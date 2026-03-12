namespace WorkflowEngine.Models;

public class Workflow
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public Trigger? Trigger { get; set; }
    public List<Condition> Conditions { get; set; } = [];
    public List<WorkflowAction> Actions { get; set; } = [];
    public bool ContinueOnError { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Trigger
{
    public string Type { get; set; } = string.Empty;
    public string? Path { get; set; }
}

public class Condition
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class WorkflowAction
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = [];
}
