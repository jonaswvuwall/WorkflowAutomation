namespace WorkflowEngine.Models;

public class ActionDefinition
{
    public string                     Id           { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string                     Name         { get; set; } = string.Empty;
    public string                     ModuleId     { get; set; } = string.Empty;
    public Dictionary<string, string> Config       { get; set; } = [];
    public string?                    NextActionId { get; set; }
    public NodePosition               Position     { get; set; } = new();
}
