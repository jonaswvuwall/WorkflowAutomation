using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public sealed class FileWatcherService(
    JsonDataService data,
    ActionExecutor actionExecutor,
    ILogger<FileWatcherService> logger) : IHostedService, IDisposable
{
    private readonly Dictionary<string, FileSystemWatcher> _watchers = [];

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

    public void Register(Workflow workflow)
    {
        Unregister(workflow.Id);

        if (workflow.Enabled &&
            (workflow.When.Type == "file_created" || workflow.When.Type == "file_modified") &&
            !string.IsNullOrWhiteSpace(workflow.When.Path))
        {
            StartWatcher(workflow);
        }
    }

    public void Unregister(string workflowId)
    {
        if (_watchers.Remove(workflowId, out var w))
            w.Dispose();
    }

    private void StartWatcher(Workflow workflow)
    {
        var path = workflow.When.Path!;

        string watchDir;
        string filter;

        if (File.Exists(path))
        {
            watchDir = Path.GetDirectoryName(path)!;
            filter = Path.GetFileName(path);
        }
        else if (Directory.Exists(path))
        {
            watchDir = path;
            filter = "*.*";
        }
        else
        {
            logger.LogWarning("Watcher path does not exist, skipping: {Path}", path);
            return;
        }

        var watcher = new FileSystemWatcher(watchDir)
        {
            Filter = filter,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        if (workflow.When.Type == "file_created")
            watcher.Created += (_, e) => OnFileEvent(workflow, e.FullPath);
        else
            watcher.Changed += (_, e) => OnFileEvent(workflow, e.FullPath);

        _watchers[workflow.Id] = watcher;
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
        foreach (var w in _watchers.Values) w.Dispose();
        _watchers.Clear();
    }
}
