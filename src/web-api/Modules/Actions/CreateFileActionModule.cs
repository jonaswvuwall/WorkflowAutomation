namespace WorkflowEngine.Modules.Actions;

public sealed class CreateFileActionModule : IActionModule
{
    public string ModuleId => "action.create_file";

    public ModuleManifest Manifest => new()
    {
        Id          = "action.create_file",
        Name        = "Create File",
        Description = "Creates a file at the specified path with optional content",
        Category    = "File System",
        Parameters  =
        [
            new ParameterSchema { Key = "path",    Label = "File Path", Type = "text",     Required = true  },
            new ParameterSchema { Key = "content", Label = "Content",   Type = "textarea", Required = false }
        ]
    };

    public async Task<NodeExecutionResult> ExecuteAsync(
        string nodeId, Dictionary<string, string> config, TriggerContext context)
    {
        var p = new ModuleParameters(config);
        try
        {
            var path    = p.Require("path");
            var content = p.Get("content", string.Empty);
            var dir     = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            await File.WriteAllTextAsync(path, content);
            return Ok(nodeId, $"Created {path}");
        }
        catch (Exception ex) { return Fail(nodeId, ex.Message); }
    }

    private NodeExecutionResult Ok(string nodeId, string msg)   => new() { NodeId = nodeId, ModuleId = ModuleId, Status = "success", Message = msg };
    private NodeExecutionResult Fail(string nodeId, string msg) => new() { NodeId = nodeId, ModuleId = ModuleId, Status = "failed",  Message = msg };
}
