using WorkflowEngine.Models;
using WorkflowEngine.Modules;

namespace WorkflowEngine.Services;

/// <summary>
/// IHostedService that registers all enabled events on startup.
/// When an event fires, executes the action graph (supporting parallel branches) and saves a Run.
/// </summary>
public sealed class WorkflowDispatcher(
    JsonDataService data,
    ModuleRegistry  registry,
    ILogger<WorkflowDispatcher> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var evt in data.GetAllEvents().Where(e => e.Enabled))
            RegisterInternal(evt);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>Call after creating or updating an event to re-register its listener.</summary>
    public void Register(EventDefinition evt)
    {
        UnregisterInternal(evt.Id);
        if (evt.Enabled)
            RegisterInternal(evt);
    }

    /// <summary>Call before deleting an event to remove its listener.</summary>
    public void Unregister(string eventId) => UnregisterInternal(eventId);

    private void RegisterInternal(EventDefinition evt)
    {
        var module = registry.GetEvent(evt.ModuleId);
        if (module is null)
        {
            logger.LogWarning("No event module for '{ModuleId}' (event {Id}: {Name})",
                evt.ModuleId, evt.Id, evt.Name);
            return;
        }

        module.Register(
            eventId: evt.Id,
            config:  evt.Config,
            onFired: data => OnEventFired(new TriggerContext
            {
                EventId       = evt.Id,
                EventName     = evt.Name,
                EventModuleId = evt.ModuleId,
                Data          = data
            }, evt));
    }

    private void UnregisterInternal(string eventId)
    {
        foreach (var module in registry.AllEvents)
            module.Unregister(eventId);
    }

    /// <summary>Execute the action graph starting at evt.FirstActionIds and save a Run.</summary>
    public async Task OnEventFired(TriggerContext context, EventDefinition evt)
    {
        var actionsById = data.GetAllActions().ToDictionary(a => a.Id);
        var allResults  = new System.Collections.Concurrent.ConcurrentBag<ActionExecutionResult>();

        var branchTasks = evt.FirstActionIds.Select(id => ExecuteBranchAsync(id, actionsById, context, allResults));
        var statuses    = await Task.WhenAll(branchTasks);

        var status = statuses.All(s => s == "success") ? "success" : "failed";

        var run = new Run
        {
            EventId       = evt.Id,
            EventName     = evt.Name,
            TriggeredAt   = DateTime.UtcNow,
            Status        = status,
            ActionResults = [.. allResults]
        };

        data.AddRun(run);
        logger.LogInformation("Run for event '{Name}': {Status} ({Count} action(s))",
            evt.Name, status, allResults.Count);
    }

    private async Task<string> ExecuteBranchAsync(
        string actionId,
        Dictionary<string, ActionDefinition> actionsById,
        TriggerContext context,
        System.Collections.Concurrent.ConcurrentBag<ActionExecutionResult> allResults)
    {
        var current = actionId;
        while (current is not null)
        {
            if (!actionsById.TryGetValue(current, out var actionDef))
            {
                logger.LogWarning("Action '{ActionId}' not found", current);
                return "failed";
            }

            var module = registry.GetAction(actionDef.ModuleId);
            if (module is null)
            {
                logger.LogWarning("No action module for '{ModuleId}' (action {ActionId})", actionDef.ModuleId, current);
                allResults.Add(new ActionExecutionResult
                {
                    ActionId = current,
                    ModuleId = actionDef.ModuleId,
                    Status   = "failed",
                    Message  = $"Unknown module '{actionDef.ModuleId}'"
                });
                return "failed";
            }

            try
            {
                var result = await module.ExecuteAsync(actionDef.Config, context);
                allResults.Add(new ActionExecutionResult
                {
                    ActionId = current,
                    ModuleId = actionDef.ModuleId,
                    Status   = result.Success ? "success" : "failed",
                    Message  = result.Message
                });

                if (!result.Success)
                    return "failed";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Action module {ModuleId} threw (action {ActionId})", actionDef.ModuleId, current);
                allResults.Add(new ActionExecutionResult
                {
                    ActionId = current,
                    ModuleId = actionDef.ModuleId,
                    Status   = "failed",
                    Message  = ex.Message
                });
                return "failed";
            }

            var nextIds = actionDef.NextActionIds;
            if (nextIds.Count == 0) break;
            if (nextIds.Count == 1)
            {
                current = nextIds[0];
            }
            else
            {
                // Fan out: run all branches in parallel
                var subTasks = nextIds.Select(id => ExecuteBranchAsync(id, actionsById, context, allResults));
                var results  = await Task.WhenAll(subTasks);
                return results.All(s => s == "success") ? "success" : "failed";
            }
        }

        return "success";
    }
}
