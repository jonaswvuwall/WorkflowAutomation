using WorkflowEngine.Modules;

namespace WorkflowEngine.Models;

public class Run
{
    public string   Id            { get; set; } = Guid.NewGuid().ToString();
    public string   EventId       { get; set; } = string.Empty;
    public string   EventName     { get; set; } = string.Empty;
    public DateTime TriggeredAt   { get; set; } = DateTime.UtcNow;
    public string   Status        { get; set; } = "pending";
    public List<NodeExecutionResult> ActionResults { get; set; } = [];
    public string?  Error         { get; set; }
}
