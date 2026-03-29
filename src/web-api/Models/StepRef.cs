namespace WorkflowEngine.Models;

public class StepRef
{
    /// <summary>Backend ID of the target action or condition.</summary>
    public string Id   { get; set; } = string.Empty;
    /// <summary>"action" | "condition"</summary>
    public string Type { get; set; } = string.Empty;
}
