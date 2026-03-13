namespace WorkflowEngine.Modules;

/// <summary>Returned by every action module — just success/fail and a human-readable message.</summary>
public record ActionResult(bool Success, string Message = "");

public interface IActionModule
{
    string ModuleId { get; }
    ModuleManifest Manifest { get; }

    Task<ActionResult> ExecuteAsync(Dictionary<string, string> config, TriggerContext context);
}
