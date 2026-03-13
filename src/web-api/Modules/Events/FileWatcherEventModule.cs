namespace WorkflowEngine.Modules.Events;

public sealed class FileWatcherEventModule(ILogger<FileWatcherEventModule> logger)
    : IEventModule, IDisposable
{
    private readonly Dictionary<string, FileSystemWatcher> _watchers = [];

    public string ModuleId => "event.file_watcher";

    public ModuleManifest Manifest => new()
    {
        Id          = "event.file_watcher",
        Name        = "File Watcher",
        Description = "Watches a file system path for changes",
        Category    = "File System",
        Parameters  =
        [
            new ParameterSchema { Key = "path",  Label = "Watch Path", Type = "text",   Required = true },
            new ParameterSchema
            {
                Key      = "event",
                Label    = "Event",
                Type     = "select",
                Required = true,
                Options  =
                [
                    new SelectOption { Value = "created",  Label = "File Created"  },
                    new SelectOption { Value = "modified", Label = "File Modified" }
                ]
            }
        ]
    };

    public void Register(string eventId, string eventName,
                         Dictionary<string, string> config,
                         Func<TriggerContext, Task> onFired)
    {
        Unregister(eventId);

        if (!config.TryGetValue("path", out var path) || string.IsNullOrWhiteSpace(path))
        {
            logger.LogWarning("FileWatcher event {EventId} ({Name}): no path configured", eventId, eventName);
            return;
        }

        config.TryGetValue("event", out var eventType);
        eventType ??= "created";

        string watchDir;
        string filter;

        if (File.Exists(path))
        {
            watchDir = Path.GetDirectoryName(path)!;
            filter   = Path.GetFileName(path);
        }
        else if (Directory.Exists(path))
        {
            watchDir = path;
            filter   = "*.*";
        }
        else
        {
            logger.LogWarning("Watcher path does not exist, skipping: {Path}", path);
            return;
        }

        var watcher = new FileSystemWatcher(watchDir)
        {
            Filter              = filter,
            NotifyFilter        = NotifyFilters.FileName | NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        FileSystemEventHandler handler = (_, e) =>
        {
            Task.Run(async () =>
            {
                try
                {
                    var ctx = new TriggerContext
                    {
                        EventId       = eventId,
                        EventName     = eventName,
                        EventModuleId = ModuleId,
                        Data          = new Dictionary<string, string>
                        {
                            ["filePath"] = e.FullPath,
                            ["fileName"] = Path.GetFileName(e.FullPath)
                        }
                    };
                    await onFired(ctx);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in FileWatcher callback for event {EventId}", eventId);
                }
            });
        };

        if (eventType == "created")
            watcher.Created += handler;
        else
            watcher.Changed += handler;

        _watchers[eventId] = watcher;
        logger.LogInformation("Watching {Path} for {Event} (event: {EventId}, name: {Name})",
            path, eventType, eventId, eventName);
    }

    public void Unregister(string eventId)
    {
        if (_watchers.Remove(eventId, out var w))
            w.Dispose();
    }

    public void Dispose()
    {
        foreach (var w in _watchers.Values) w.Dispose();
        _watchers.Clear();
    }
}
