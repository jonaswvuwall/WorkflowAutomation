using WorkflowEngine.Models;
using WorkflowEngine.Modules;

namespace WorkflowEngine.Services;

/// <summary>
/// IHostedService that registers all enabled events on startup.
/// When an event fires, executes the step graph (actions + conditions) and saves a Run.
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

    public void Register(EventDefinition evt)
    {
        UnregisterInternal(evt.Id);
        if (evt.Enabled)
            RegisterInternal(evt);
    }

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
            onFired: triggerData => OnEventFired(new TriggerContext
            {
                EventId       = evt.Id,
                EventName     = evt.Name,
                EventModuleId = evt.ModuleId,
                Data          = triggerData
            }, evt));
    }

    private void UnregisterInternal(string eventId)
    {
        foreach (var module in registry.AllEvents)
            module.Unregister(eventId);
    }

    // ── Execution entry point ─────────────────────────────────────────────────

    public async Task OnEventFired(TriggerContext context, EventDefinition evt)
    {
        var actionsById    = data.GetAllActions().ToDictionary(a => a.Id);
        var conditionsById = data.GetAllConditions().ToDictionary(c => c.Id);

        var actionResults    = new System.Collections.Concurrent.ConcurrentBag<ActionExecutionResult>();
        var conditionResults = new System.Collections.Concurrent.ConcurrentBag<ConditionStepResult>();

        var branchTasks = evt.FirstSteps.Select(step =>
            ExecuteStepAsync(step, actionsById, conditionsById, context, actionResults, conditionResults));
        var statuses = await Task.WhenAll(branchTasks);

        var status = statuses.Length == 0 || statuses.All(s => s == "success") ? "success" : "failed";

        var run = new Run
        {
            EventId          = evt.Id,
            EventName        = evt.Name,
            TriggeredAt      = DateTime.UtcNow,
            Status           = status,
            ActionResults    = [.. actionResults],
            ConditionResults = [.. conditionResults]
        };

        data.AddRun(run);
        logger.LogInformation("Run for event '{Name}': {Status} ({A} action(s), {C} condition(s))",
            evt.Name, status, actionResults.Count, conditionResults.Count);
    }

    // ── Template substitution ─────────────────────────────────────────────────

    /// <summary>
    /// Replaces {{key}} placeholders in every config value with the matching entry from context.Data.
    /// Unresolved placeholders are left as-is.
    /// </summary>
    private static Dictionary<string, string> ApplyTemplates(
        Dictionary<string, string> config, TriggerContext context)
    {
        if (context.Data.Count == 0) return config;

        var result = new Dictionary<string, string>(config.Count);
        foreach (var (k, v) in config)
        {
            result[k] = System.Text.RegularExpressions.Regex.Replace(
                v, @"\{\{(\w+)\}\}",
                m => context.Data.TryGetValue(m.Groups[1].Value, out var val) ? val : m.Value);
        }
        return result;
    }

    // ── Step router ───────────────────────────────────────────────────────────

    private Task<string> ExecuteStepAsync(
        StepRef step,
        Dictionary<string, ActionDefinition>    actionsById,
        Dictionary<string, ConditionDefinition> conditionsById,
        TriggerContext context,
        System.Collections.Concurrent.ConcurrentBag<ActionExecutionResult> actionResults,
        System.Collections.Concurrent.ConcurrentBag<ConditionStepResult>  conditionResults)
    => step.Type switch
    {
        "action"    => ExecuteActionChainAsync(step.Id, actionsById, conditionsById, context, actionResults, conditionResults),
        "condition" => ExecuteConditionAsync(step.Id, actionsById, conditionsById, context, actionResults, conditionResults),
        _           => Task.FromResult("failed")
    };

    private async Task<string> ExecuteNextStepsAsync(
        IReadOnlyList<StepRef> nextSteps,
        Dictionary<string, ActionDefinition>    actionsById,
        Dictionary<string, ConditionDefinition> conditionsById,
        TriggerContext context,
        System.Collections.Concurrent.ConcurrentBag<ActionExecutionResult> actionResults,
        System.Collections.Concurrent.ConcurrentBag<ConditionStepResult>  conditionResults)
    {
        if (nextSteps.Count == 0) return "success";
        if (nextSteps.Count == 1)
            return await ExecuteStepAsync(nextSteps[0], actionsById, conditionsById, context, actionResults, conditionResults);

        var tasks   = nextSteps.Select(s => ExecuteStepAsync(s, actionsById, conditionsById, context, actionResults, conditionResults));
        var results = await Task.WhenAll(tasks);
        return results.All(s => s == "success") ? "success" : "failed";
    }

    // ── Action chain ──────────────────────────────────────────────────────────

    private async Task<string> ExecuteActionChainAsync(
        string actionId,
        Dictionary<string, ActionDefinition>    actionsById,
        Dictionary<string, ConditionDefinition> conditionsById,
        TriggerContext context,
        System.Collections.Concurrent.ConcurrentBag<ActionExecutionResult> actionResults,
        System.Collections.Concurrent.ConcurrentBag<ConditionStepResult>  conditionResults)
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
                actionResults.Add(new ActionExecutionResult
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
                var resolvedConfig = ApplyTemplates(actionDef.Config, context);
                var result = await module.ExecuteAsync(resolvedConfig, context);

                // Merge any output values into the shared context for downstream steps
                if (result.OutputData is not null)
                    foreach (var (k, v) in result.OutputData)
                        context.Data[k] = v;

                actionResults.Add(new ActionExecutionResult
                {
                    ActionId = current,
                    ModuleId = actionDef.ModuleId,
                    Status   = result.Success ? "success" : "failed",
                    Message  = result.Message
                });

                if (!result.Success) return "failed";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Action module {ModuleId} threw (action {ActionId})", actionDef.ModuleId, current);
                actionResults.Add(new ActionExecutionResult
                {
                    ActionId = current,
                    ModuleId = actionDef.ModuleId,
                    Status   = "failed",
                    Message  = ex.Message
                });
                return "failed";
            }

            var nextSteps = actionDef.NextSteps;
            if (nextSteps.Count == 0) break;

            // Stay in the loop if the sole next step is another action (avoids recursion)
            if (nextSteps.Count == 1 && nextSteps[0].Type == "action")
            {
                current = nextSteps[0].Id;
                continue;
            }

            return await ExecuteNextStepsAsync(nextSteps, actionsById, conditionsById, context, actionResults, conditionResults);
        }

        return "success";
    }

    // ── Condition ─────────────────────────────────────────────────────────────

    private async Task<string> ExecuteConditionAsync(
        string conditionId,
        Dictionary<string, ActionDefinition>    actionsById,
        Dictionary<string, ConditionDefinition> conditionsById,
        TriggerContext context,
        System.Collections.Concurrent.ConcurrentBag<ActionExecutionResult> actionResults,
        System.Collections.Concurrent.ConcurrentBag<ConditionStepResult>  conditionResults)
    {
        if (!conditionsById.TryGetValue(conditionId, out var condDef))
        {
            logger.LogWarning("Condition '{ConditionId}' not found", conditionId);
            return "failed";
        }

        var module = registry.GetCondition(condDef.ModuleId);
        if (module is null)
        {
            logger.LogWarning("No condition module for '{ModuleId}' (condition {ConditionId})", condDef.ModuleId, conditionId);
            conditionResults.Add(new ConditionStepResult
            {
                ConditionId = conditionId,
                ModuleId    = condDef.ModuleId,
                Result      = false,
                Message     = $"Unknown module '{condDef.ModuleId}'"
            });
            return "failed";
        }

        bool passed;
        try
        {
            var resolvedConfig = ApplyTemplates(condDef.Config, context);
            passed = await module.EvaluateAsync(resolvedConfig, context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Condition module {ModuleId} threw (condition {ConditionId})", condDef.ModuleId, conditionId);
            conditionResults.Add(new ConditionStepResult
            {
                ConditionId = conditionId,
                ModuleId    = condDef.ModuleId,
                Result      = false,
                Message     = ex.Message
            });
            return "failed";
        }

        conditionResults.Add(new ConditionStepResult
        {
            ConditionId = conditionId,
            ModuleId    = condDef.ModuleId,
            Result      = passed,
            Message     = passed ? "Passed" : "Did not pass"
        });

        logger.LogInformation("Condition {ConditionId} ({ModuleId}): {Result}",
            conditionId, condDef.ModuleId, passed ? "passed" : "skipped");

        // A condition branch ending with no further steps is still a success
        var nextSteps = passed ? condDef.TrueNextSteps : condDef.FalseNextSteps;
        return await ExecuteNextStepsAsync(nextSteps, actionsById, conditionsById, context, actionResults, conditionResults);
    }
}
