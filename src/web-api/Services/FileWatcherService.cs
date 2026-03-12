using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public sealed class FileWatcherService(
    JsonDataService data,
    ActionExecutor actionExecutor,
    ILogger<FileWatcherService> logger) : IHostedService, IDisposable
{
    private readonly List<FileSystemWatcher> _watchers = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var workflows = data.GetWorkflows()
            .Where(w => w.Enabled &&
                        (w.When.Type == "file_created" || w.When.Type == "file_modified") &&
                        !string.IsNullOrWhiteSpace(w.When.Path));

        foreach (var workflow in workflows)
            StartWatcher(workflow);

        return Task.CompletedTask;
    }

    private void StartWatcher(Workflow workflow)
    {
        var path = workflow.When.Path!;
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

        if (workflow.When.Type == "file_created")
            watcher.Created += (_, e) => OnFileEvent(workflow, e.FullPath);
        else
            watcher.Changed += (_, e) => OnFileEvent(workflow, e.FullPath);

        _watchers.Add(watcher);
        logger.LogInformation("Watching {Path} for {Type} (workflow: {Name})", path, workflow.When.Type, workflow.Name);
    }

    private void OnFileEvent(Workflow workflow, string fullPath)
    {
        Task.Run(async () =>
        {
            try
            {
                var context = TriggerContext.FromFile(workflow.When.Type, fullPath);
                var result = await actionExecutor.ExecuteAsync(workflow.Then, context);

                var run = new Run
                {
                    WorkflowId = workflow.Id,
                    TriggeredAt = DateTime.UtcNow,
                    ActionExecuted = result,
                    Status = result.Status
                };
                data.AddRun(run);
                logger.LogInformation("Auto-run for workflow {Name}: status={Status}", workflow.Name, run.Status);
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
