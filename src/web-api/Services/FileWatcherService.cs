using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public sealed class FileWatcherService(
    JsonDataService data,
    ConditionEvaluator conditionEvaluator,
    ActionExecutor actionExecutor,
    ILogger<FileWatcherService> logger) : IHostedService, IDisposable
{
    private readonly List<FileSystemWatcher> _watchers = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var workflows = data.GetWorkflows()
            .Where(w => w.Enabled && w.Trigger is not null &&
                        (w.Trigger.Type == "file_created" || w.Trigger.Type == "file_modified") &&
                        !string.IsNullOrWhiteSpace(w.Trigger.Path));

        foreach (var workflow in workflows)
            StartWatcher(workflow);

        return Task.CompletedTask;
    }

    private void StartWatcher(Workflow workflow)
    {
        var path = workflow.Trigger!.Path!;
        if (!Directory.Exists(path))
        {
            logger.LogWarning("Watcher path does not exist, skipping: {Path}", path);
            return;
        }

        var watcher = new FileSystemWatcher(path)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        if (workflow.Trigger.Type == "file_created")
            watcher.Created += (_, e) => OnFileEvent(workflow, e.FullPath);
        else
            watcher.Changed += (_, e) => OnFileEvent(workflow, e.FullPath);

        _watchers.Add(watcher);
        logger.LogInformation("Watching {Path} for {Type} (workflow: {Name})", path, workflow.Trigger.Type, workflow.Name);
    }

    private void OnFileEvent(Workflow workflow, string fullPath)
    {
        Task.Run(async () =>
        {
            try
            {
                var context = TriggerContext.FromFile(workflow.Trigger!.Type, fullPath);
                var conditionsMet = conditionEvaluator.Evaluate(workflow.Conditions, context);

                var actionResults = new List<ActionResult>();
                var status = "success";

                if (conditionsMet)
                {
                    foreach (var action in workflow.Actions)
                    {
                        var result = await actionExecutor.ExecuteAsync(action, context);
                        actionResults.Add(result);
                        if (result.Status == "failed" && !workflow.ContinueOnError)
                        {
                            status = "failed";
                            break;
                        }
                        if (result.Status == "failed") status = "failed";
                    }
                }

                var run = new Run
                {
                    WorkflowId = workflow.Id,
                    TriggeredAt = DateTime.UtcNow,
                    ConditionsMet = conditionsMet,
                    ActionsExecuted = actionResults,
                    Status = conditionsMet ? status : "success"
                };
                data.AddRun(run);
                logger.LogInformation("Auto-run for workflow {Name}: conditionsMet={Met}, status={Status}",
                    workflow.Name, conditionsMet, run.Status);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during auto-run for workflow {Id}", workflow.Id);
            }
        });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        foreach (var w in _watchers) w.Dispose();
        _watchers.Clear();
    }
}
