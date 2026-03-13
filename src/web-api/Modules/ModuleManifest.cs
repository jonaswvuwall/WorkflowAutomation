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
    /// <summary>Pre-filled value when a new node is created from this template.</summary>
    public string? Default { get; set; }
    public List<SelectOption> Options { get; set; } = [];
    /// <summary>If set, this field is only shown when the named key equals the given value.</summary>
    public VisibleWhen? VisibleWhen { get; set; }
}

public class VisibleWhen
{
    public string Key   { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
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
