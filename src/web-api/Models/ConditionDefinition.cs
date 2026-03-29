namespace WorkflowEngine.Models;

public class ConditionDefinition
{
    public string                     Id             { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string                     Name           { get; set; } = string.Empty;
    public string                     ModuleId       { get; set; } = string.Empty;
    public Dictionary<string, string> Config         { get; set; } = [];
    public List<StepRef>              TrueNextSteps  { get; set; } = [];
    public List<StepRef>              FalseNextSteps { get; set; } = [];
    public NodeUi                     Ui             { get; set; } = new();
}
