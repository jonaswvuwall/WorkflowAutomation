using WorkflowEngine.Modules;

namespace WorkflowEngine.Models;

public class CustomModuleDefinition
{
    public string   Id            { get; set; } = string.Empty;
    public string   Name          { get; set; } = string.Empty;
    public string   Description   { get; set; } = string.Empty;
    public string   Category      { get; set; } = "Custom";
    /// <summary>"event" | "action"</summary>
    public string   ModuleType    { get; set; } = "action";
    /// <summary>"script" | "http_request"</summary>
    public string   BaseType      { get; set; } = "script";

    // Script-based: {{paramKey}} is substituted with config values
    public string?  ScriptContent { get; set; }

    // HTTP-based
    public string?  HttpMethod    { get; set; } = "POST";
    public string?  HttpUrl       { get; set; }
    public string?  HttpBody      { get; set; }

    public List<ParameterSchema> Parameters { get; set; } = [];
}
