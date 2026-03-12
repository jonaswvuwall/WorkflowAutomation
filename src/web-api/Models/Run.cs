namespace WorkflowEngine.Models;

public class Run
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string WorkflowId { get; set; } = string.Empty;
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "pending";
    public bool ConditionsMet { get; set; } = true;
    public List<ActionResult> ActionsExecuted { get; set; } = [];
    public string? Error { get; set; }
}

public class ActionResult
{
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}
