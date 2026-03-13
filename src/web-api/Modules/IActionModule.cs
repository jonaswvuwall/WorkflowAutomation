namespace WorkflowEngine.Modules;

public class NodeExecutionResult
{
    public string  NodeId    { get; set; } = string.Empty;
    public string  ModuleId  { get; set; } = string.Empty;
    public string  Status    { get; set; } = string.Empty;  // "success" | "failed"
    public string? Message   { get; set; }
}

public interface IActionModule
{
    string ModuleId { get; }
    ModuleManifest Manifest { get; }

    Task<NodeExecutionResult> ExecuteAsync(
        string nodeId,
        Dictionary<string, string> config,
        TriggerContext context);
}
