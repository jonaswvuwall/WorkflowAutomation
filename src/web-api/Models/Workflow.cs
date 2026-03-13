namespace WorkflowEngine.Models;

public class Workflow
{
    public string             Id        { get; set; } = Guid.NewGuid().ToString();
    public string             Name      { get; set; } = string.Empty;
    public bool               Enabled   { get; set; } = true;
    public List<WorkflowNode> Nodes     { get; set; } = [];
    public List<WorkflowEdge> Edges     { get; set; } = [];
    public DateTime           CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime           UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class WorkflowNode
{
    public string                     Id       { get; set; } = Guid.NewGuid().ToString();
    /// <summary>"event" | "execution" | "action"</summary>
    public string                     Type     { get; set; } = string.Empty;
    public string                     ModuleId { get; set; } = string.Empty;
    public Dictionary<string, string> Config   { get; set; } = [];
    public NodePosition               Position { get; set; } = new();
}

public class WorkflowEdge
{
    public string Id     { get; set; } = Guid.NewGuid().ToString();
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
}

public class NodePosition
{
    public double X { get; set; }
    public double Y { get; set; }
}
