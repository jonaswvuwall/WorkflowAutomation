namespace WorkflowEngine.Modules.Conditions;

public sealed class ValueContainsConditionModule : IConditionModule
{
    public string ModuleId => "condition.value_contains";

    public ModuleManifest Manifest => new()
    {
        Id          = "condition.value_contains",
        Name        = "Value Contains",
        Description = "Passes when a context variable contains a given substring",
        Category    = "Data",
        Parameters  =
        [
            new ParameterSchema
            {
                Key      = "key",
                Label    = "Context Key",
                Type     = "text",
                Required = true,
                Default  = ""
            },
            new ParameterSchema
            {
                Key      = "value",
                Label    = "Substring",
                Type     = "text",
                Required = true,
                Default  = ""
            },
            new ParameterSchema
            {
                Key      = "ignoreCase",
                Label    = "Ignore Case",
                Type     = "select",
                Required = false,
                Default  = "false",
                Options  =
                [
                    new SelectOption { Value = "false", Label = "No" },
                    new SelectOption { Value = "true",  Label = "Yes" }
                ]
            }
        ]
    };

    public Task<bool> EvaluateAsync(Dictionary<string, string> config, TriggerContext context)
    {
        config.TryGetValue("key",        out var key);
        config.TryGetValue("value",      out var substring);
        config.TryGetValue("ignoreCase", out var ignoreCaseStr);

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(substring))
            return Task.FromResult(false);

        context.Data.TryGetValue(key, out var actual);
        actual ??= string.Empty;

        var comparison = ignoreCaseStr == "true"
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return Task.FromResult(actual.Contains(substring, comparison));
    }
}
