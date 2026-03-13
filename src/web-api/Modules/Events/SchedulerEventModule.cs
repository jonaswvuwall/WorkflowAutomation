namespace WorkflowEngine.Modules.Events;

public sealed class SchedulerEventModule(ILogger<SchedulerEventModule> logger)
    : IEventModule, IDisposable
{
    private readonly Dictionary<string, CancellationTokenSource> _timers = [];

    public string ModuleId => "event.scheduler";

    public ModuleManifest Manifest => new()
    {
        Id          = "event.scheduler",
        Name        = "Scheduler",
        Description = "Fires on a fixed interval or cron schedule",
        Category    = "General",
        Parameters  =
        [
            new ParameterSchema
            {
                Key      = "type",
                Label    = "Schedule Type",
                Type     = "select",
                Required = true,
                Default  = "interval",
                Options  =
                [
                    new SelectOption { Value = "interval", Label = "Interval (minutes)" },
                    new SelectOption { Value = "cron",     Label = "Cron Expression"    }
                ]
            },
            new ParameterSchema { Key = "every",      Label = "Every (minutes)", Type = "number", Required = false, Default = "5",         VisibleWhen = new VisibleWhen { Key = "type", Value = "interval" } },
            new ParameterSchema { Key = "expression", Label = "Cron Expression", Type = "text",   Required = false, Default = "0 * * * *", VisibleWhen = new VisibleWhen { Key = "type", Value = "cron"     } }
        ]
    };

    public void Register(string eventId, Dictionary<string, string> config,
                         Func<Dictionary<string, string>, Task> onFired)
    {
        Unregister(eventId);
        var cts = new CancellationTokenSource();
        _timers[eventId] = cts;

        config.TryGetValue("type", out var type);
        if (type == "cron")
            Task.Run(() => RunCron(eventId, config, onFired, cts.Token));
        else
            Task.Run(() => RunInterval(eventId, config, onFired, cts.Token));
    }

    public void Unregister(string eventId)
    {
        if (_timers.Remove(eventId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    private async Task RunInterval(string eventId, Dictionary<string, string> config,
                                   Func<Dictionary<string, string>, Task> onFired,
                                   CancellationToken ct)
    {
        config.TryGetValue("every", out var everyStr);
        var minutes = double.TryParse(everyStr, System.Globalization.NumberStyles.Any,
                          System.Globalization.CultureInfo.InvariantCulture, out var m) && m > 0 ? m : 5;
        logger.LogInformation("Scheduler {EventId}: interval every {Minutes}m", eventId, minutes);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(minutes));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
                await Fire(eventId, onFired);
        }
        catch (OperationCanceledException) { }
    }

    private async Task RunCron(string eventId, Dictionary<string, string> config,
                               Func<Dictionary<string, string>, Task> onFired,
                               CancellationToken ct)
    {
        config.TryGetValue("expression", out var expr);
        expr = string.IsNullOrWhiteSpace(expr) ? "0 * * * *" : expr;

        Cronos.CronExpression cron;
        try   { cron = Cronos.CronExpression.Parse(expr); }
        catch { logger.LogWarning("Scheduler {EventId}: invalid cron '{Expr}'", eventId, expr); return; }

        logger.LogInformation("Scheduler {EventId}: cron '{Expr}'", eventId, expr);
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var now  = DateTime.UtcNow;
                var next = cron.GetNextOccurrence(now, TimeZoneInfo.Utc);
                if (next is null) break;

                var delay = next.Value - now;
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, ct);

                if (!ct.IsCancellationRequested)
                    await Fire(eventId, onFired);
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task Fire(string eventId, Func<Dictionary<string, string>, Task> onFired)
    {
        try
        {
            await onFired(new Dictionary<string, string>
            {
                ["scheduledAt"] = DateTime.UtcNow.ToString("O")
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Scheduler callback error for event {EventId}", eventId);
        }
    }

    public void Dispose()
    {
        foreach (var cts in _timers.Values) { cts.Cancel(); cts.Dispose(); }
        _timers.Clear();
    }
}
