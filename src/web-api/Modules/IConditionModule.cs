namespace WorkflowEngine.Modules;

public interface IConditionModule
{
    string ModuleId { get; }
    ModuleManifest Manifest { get; }

    /// <summary>Returns true if the condition passes, false otherwise.</summary>
    Task<bool> EvaluateAsync(Dictionary<string, string> config, TriggerContext context);
}
