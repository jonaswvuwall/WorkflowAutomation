namespace WorkflowEngine.Models;

public class Workflow
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public Trigger When { get; set; } = new();
    public WorkflowAction Then { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Trigger
{
    public string Type { get; set; } = string.Empty;
    public string? Path { get; set; }
}

public class WorkflowAction
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = [];
}
