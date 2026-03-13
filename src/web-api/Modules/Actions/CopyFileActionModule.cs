namespace WorkflowEngine.Modules.Actions;

public sealed class CopyFileActionModule : IActionModule
{
    public string ModuleId => "action.copy_file";

    public ModuleManifest Manifest => new()
    {
        Id          = "action.copy_file",
        Name        = "Copy File",
        Description = "Copies a file from source to destination",
        Category    = "File System",
        Parameters  =
        [
            new ParameterSchema { Key = "source",      Label = "Source Path",      Type = "text", Required = true },
            new ParameterSchema { Key = "destination", Label = "Destination Path", Type = "text", Required = true }
        ]
    };

    public Task<NodeExecutionResult> ExecuteAsync(
        string nodeId, Dictionary<string, string> config, TriggerContext context)
    {
        var p = new ModuleParameters(config);
        try
        {
            var src = p.Require("source");
            var dst = p.Require("destination");
            var dir = Path.GetDirectoryName(dst);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.Copy(src, dst, overwrite: true);
            return Task.FromResult(Ok(nodeId, $"Copied {src} → {dst}"));
        }
        catch (Exception ex) { return Task.FromResult(Fail(nodeId, ex.Message)); }
    }

    private NodeExecutionResult Ok(string nodeId, string msg)   => new() { NodeId = nodeId, ModuleId = ModuleId, Status = "success", Message = msg };
    private NodeExecutionResult Fail(string nodeId, string msg) => new() { NodeId = nodeId, ModuleId = ModuleId, Status = "failed",  Message = msg };
}
