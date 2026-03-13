namespace WorkflowEngine.Modules;

public class SelectOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class ParameterSchema
{
    public string Key      { get; set; } = string.Empty;
    public string Label    { get; set; } = string.Empty;
    /// <summary>Allowed: "text" | "textarea" | "select" | "number" | "toggle"</summary>
    public string Type     { get; set; } = "text";
    public bool   Required { get; set; }
    public List<SelectOption> Options { get; set; } = [];
}

public class ModuleManifest
{
    public string Id          { get; set; } = string.Empty;
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category    { get; set; } = string.Empty;
    public List<ParameterSchema> Parameters { get; set; } = [];
}

/// <summary>Runtime data passed from an event through the action chain.</summary>
public class TriggerContext
{
    public string EventId       { get; set; } = string.Empty;
    public string EventName     { get; set; } = string.Empty;
    public string EventModuleId { get; set; } = string.Empty;
    /// <summary>Key/value data from the event, e.g. "filePath" → "C:\foo\bar.txt"</summary>
    public Dictionary<string, string> Data { get; set; } = [];
}
