namespace WorkflowEngine.Modules.Events;

public sealed class CalendarEventModule(ILogger<CalendarEventModule> logger)
    : IEventModule, IDisposable
{
    private readonly Dictionary<string, CancellationTokenSource> _timers = [];

    public string ModuleId => "event.calendar";

    public ModuleManifest Manifest => new()
    {
        Id          = "event.calendar",
        Name        = "Schedule by Calendar",
        Description = "Fires on specific days of the week at a set time",
        Category    = "General",
        Parameters  =
        [
            new ParameterSchema
            {
                Key      = "days",
                Label    = "Days (e.g. mon,wed,fri)",
                Type     = "text",
                Required = true,
                Default  = "mon,tue,wed,thu,fri"
            },
            new ParameterSchema
            {
                Key      = "time",
                Label    = "Time (HH:mm)",
                Type     = "text",
                Required = true,
                Default  = "09:00"
            }
        ]
    };

    public void Register(string eventId, Dictionary<string, string> config,
                         Func<Dictionary<string, string>, Task> onFired)
    {
        Unregister(eventId);
        var cts = new CancellationTokenSource();
        _timers[eventId] = cts;
        Task.Run(() => RunCalendar(eventId, config, onFired, cts.Token));
    }

    public void Unregister(string eventId)
    {
        if (_timers.Remove(eventId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    private async Task RunCalendar(string eventId, Dictionary<string, string> config,
                                   Func<Dictionary<string, string>, Task> onFired,
                                   CancellationToken ct)
    {
        config.TryGetValue("days", out var daysRaw);
        config.TryGetValue("time", out var timeRaw);

        var cronExpr = BuildCron(daysRaw ?? "", timeRaw ?? "09:00");
        if (cronExpr is null)
        {
            logger.LogWarning("Calendar event {EventId}: invalid days/time config (days='{Days}' time='{Time}')",
                eventId, daysRaw, timeRaw);
            return;
        }

        Cronos.CronExpression cron;
        try { cron = Cronos.CronExpression.Parse(cronExpr); }
        catch
        {
            logger.LogWarning("Calendar event {EventId}: failed to parse cron '{Expr}'", eventId, cronExpr);
            return;
        }

        logger.LogInformation("Calendar event {EventId}: cron '{Expr}'", eventId, cronExpr);

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
                    await Fire(eventId, onFired, next.Value);
            }
        }
        catch (OperationCanceledException) { }
    }

    /// <summary>Converts day names + HH:mm into a 5-field cron expression.</summary>
    private static string? BuildCron(string daysRaw, string timeRaw)
    {
        if (!TimeOnly.TryParse(timeRaw, out var time))
            return null;

        var dayMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["sun"] = 0, ["sunday"]    = 0,
            ["mon"] = 1, ["monday"]    = 1,
            ["tue"] = 2, ["tuesday"]   = 2,
            ["wed"] = 3, ["wednesday"] = 3,
            ["thu"] = 4, ["thursday"]  = 4,
            ["fri"] = 5, ["friday"]    = 5,
            ["sat"] = 6, ["saturday"]  = 6
        };

        var dayNumbers = daysRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(d => dayMap.TryGetValue(d, out var n) ? (int?)n : null)
            .Where(n => n.HasValue)
            .Select(n => n!.Value)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        if (dayNumbers.Count == 0)
            return null;

        var daysPart = dayNumbers.Count == 7 ? "*" : string.Join(",", dayNumbers);
        return $"{time.Minute} {time.Hour} * * {daysPart}";
    }

    private async Task Fire(string eventId, Func<Dictionary<string, string>, Task> onFired, DateTime scheduledAt)
    {
        try
        {
            await onFired(new Dictionary<string, string>
            {
                ["scheduledAt"] = scheduledAt.ToString("O")
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Calendar callback error for event {EventId}", eventId);
        }
    }

    public void Dispose()
    {
        foreach (var cts in _timers.Values) { cts.Cancel(); cts.Dispose(); }
        _timers.Clear();
    }
}
