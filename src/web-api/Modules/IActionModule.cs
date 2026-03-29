namespace WorkflowEngine.Modules;

/// <summary>
/// Returned by every action module.
/// OutputData entries are merged into TriggerContext.Data so later steps can consume them via {{key}} templates.
/// </summary>
public record ActionResult(
    bool Success,
    string Message = "",
    Dictionary<string, string>? OutputData = null);

public interface IActionModule
{
    string ModuleId { get; }
    ModuleManifest Manifest { get; }

    Task<ActionResult> ExecuteAsync(Dictionary<string, string> config, TriggerContext context);
}
