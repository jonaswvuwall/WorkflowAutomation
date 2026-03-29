namespace WorkflowEngine.Modules.Conditions;

public sealed class TimeInRangeConditionModule : IConditionModule
{
    public string ModuleId => "condition.time_in_range";

    public ModuleManifest Manifest => new()
    {
        Id          = "condition.time_in_range",
        Name        = "Time in Range",
        Description = "Passes when the current UTC time falls within a given HH:mm–HH:mm window",
        Category    = "Time",
        Parameters  =
        [
            new ParameterSchema
            {
                Key      = "from",
                Label    = "From (HH:mm UTC)",
                Type     = "text",
                Required = true,
                Default  = "09:00"
            },
            new ParameterSchema
            {
                Key      = "to",
                Label    = "To (HH:mm UTC)",
                Type     = "text",
                Required = true,
                Default  = "17:00"
            }
        ]
    };

    public Task<bool> EvaluateAsync(Dictionary<string, string> config, TriggerContext context)
    {
        config.TryGetValue("from", out var fromStr);
        config.TryGetValue("to",   out var toStr);

        if (!TimeOnly.TryParse(fromStr, out var from) || !TimeOnly.TryParse(toStr, out var to))
            return Task.FromResult(false);

        var now = TimeOnly.FromDateTime(DateTime.UtcNow);

        bool inRange = from <= to
            ? now >= from && now <= to          // same-day window  e.g. 09:00–17:00
            : now >= from || now <= to;         // overnight window e.g. 22:00–06:00

        return Task.FromResult(inRange);
    }
}
