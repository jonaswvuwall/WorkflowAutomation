namespace WorkflowEngine.Modules.Conditions;

public sealed class ValueEqualsConditionModule : IConditionModule
{
    public string ModuleId => "condition.value_equals";

    public ModuleManifest Manifest => new()
    {
        Id          = "condition.value_equals",
        Name        = "Value Equals",
        Description = "Passes when a context variable exactly matches a value",
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
                Label    = "Expected Value",
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
        config.TryGetValue("value",      out var expected);
        config.TryGetValue("ignoreCase", out var ignoreCaseStr);

        if (string.IsNullOrEmpty(key)) return Task.FromResult(false);

        context.Data.TryGetValue(key, out var actual);
        actual   ??= string.Empty;
        expected ??= string.Empty;

        var comparison = ignoreCaseStr == "true"
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        return Task.FromResult(string.Equals(actual, expected, comparison));
    }
}
