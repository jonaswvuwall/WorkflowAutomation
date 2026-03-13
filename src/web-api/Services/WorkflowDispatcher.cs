using WorkflowEngine.Models;
using WorkflowEngine.Modules;

namespace WorkflowEngine.Services;

/// <summary>
/// IHostedService that registers all enabled events on startup.
/// When an event fires, walks the action chain (firstActionId → nextActionId) and saves a Run.
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

    /// <summary>Execute the action chain starting at evt.FirstActionId and save a Run.</summary>
    public async Task OnEventFired(TriggerContext context, EventDefinition evt)
    {
        var allActions = data.GetAllActions();
        var results    = new List<ActionExecutionResult>();
        var status     = "success";

        var actionId = evt.FirstActionId;
        while (actionId is not null)
        {
            var actionDef = allActions.FirstOrDefault(a => a.Id == actionId);
            if (actionDef is null)
            {
                logger.LogWarning("Action '{ActionId}' not found (event {EventId})", actionId, evt.Id);
                status = "failed";
                break;
            }

            var module = registry.GetAction(actionDef.ModuleId);
            if (module is null)
            {
                logger.LogWarning("No action module for '{ModuleId}' (action {ActionId})", actionDef.ModuleId, actionId);
                results.Add(new ActionExecutionResult { ActionId = actionId, ModuleId = actionDef.ModuleId, Status = "failed", Message = $"Unknown module '{actionDef.ModuleId}'" });
                status = "failed";
                break;
            }

            try
            {
                var result = await module.ExecuteAsync(actionDef.Config, context);
                results.Add(new ActionExecutionResult
                {
                    ActionId = actionId,
                    ModuleId = actionDef.ModuleId,
                    Status   = result.Success ? "success" : "failed",
                    Message  = result.Message
                });
                if (!result.Success)
                {
                    status = "failed";
                    break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Action module {ModuleId} threw (action {ActionId})", actionDef.ModuleId, actionId);
                results.Add(new ActionExecutionResult { ActionId = actionId, ModuleId = actionDef.ModuleId, Status = "failed", Message = ex.Message });
                status = "failed";
                break;
            }

            actionId = actionDef.NextActionId;
        }

        var run = new Run
        {
            EventId       = evt.Id,
            EventName     = evt.Name,
            TriggeredAt   = DateTime.UtcNow,
            Status        = status,
            ActionResults = results
        };

        data.AddRun(run);
        logger.LogInformation("Run for event '{Name}': {Status} ({Count} action(s))",
            evt.Name, status, results.Count);
    }
}
