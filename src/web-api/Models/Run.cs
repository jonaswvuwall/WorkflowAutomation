namespace WorkflowEngine.Models;

public class ActionExecutionResult
{
    public string  ActionId { get; set; } = string.Empty;
    public string  ModuleId { get; set; } = string.Empty;
    public string  Status   { get; set; } = string.Empty; // "success" | "failed"
    public string? Message  { get; set; }
}

public class ConditionStepResult
{
    public string ConditionId { get; set; } = string.Empty;
    public string ModuleId    { get; set; } = string.Empty;
    public bool   Result      { get; set; }
    public string Message     { get; set; } = string.Empty;
}

public class Run
{
    public string   Id               { get; set; } = Guid.NewGuid().ToString();
    public string   EventId          { get; set; } = string.Empty;
    public string   EventName        { get; set; } = string.Empty;
    public DateTime TriggeredAt      { get; set; } = DateTime.UtcNow;
    public string   Status           { get; set; } = "pending";
    public List<ActionExecutionResult> ActionResults    { get; set; } = [];
    public List<ConditionStepResult>   ConditionResults { get; set; } = [];
    public string?  Error            { get; set; }
}
