namespace WorkflowEngine.Modules.Conditions;

public sealed class AlwaysTrueConditionModule : IConditionModule
{
    public string ModuleId => "condition.always_true";

    public ModuleManifest Manifest => new()
    {
        Id          = "condition.always_true",
        Name        = "Always True",
        Description = "Always passes — useful for testing or unconditional routing",
        Category    = "General",
        Parameters  = []
    };

    public Task<bool> EvaluateAsync(Dictionary<string, string> config, TriggerContext context)
        => Task.FromResult(true);
}
