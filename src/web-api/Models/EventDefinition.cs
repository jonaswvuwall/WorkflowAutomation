namespace WorkflowEngine.Models;

public class EventDefinition
{
    public string                     Id            { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string                     Name          { get; set; } = string.Empty;
    public bool                       Enabled       { get; set; } = true;
    public string                     ModuleId      { get; set; } = string.Empty;
    public Dictionary<string, string> Config        { get; set; } = [];
    public string?                    FirstActionId { get; set; }
    public NodeUi                     Ui            { get; set; } = new();
}
