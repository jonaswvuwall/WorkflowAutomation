namespace WorkflowEngine.Modules.Actions;

public sealed class DeleteFileActionModule : IActionModule
{
    public string ModuleId => "action.delete_file";

    public ModuleManifest Manifest => new()
    {
        Id          = "action.delete_file",
        Name        = "Delete File",
        Description = "Deletes a file at the specified path",
        Category    = "File System",
        Parameters  =
        [
            new ParameterSchema { Key = "path", Label = "File Path", Type = "text", Required = true }
        ]
    };

    public Task<NodeExecutionResult> ExecuteAsync(
        string nodeId, Dictionary<string, string> config, TriggerContext context)
    {
        var p = new ModuleParameters(config);
        try
        {
            var path = p.Require("path");
            if (File.Exists(path))
                File.Delete(path);
            return Task.FromResult(Ok(nodeId, $"Deleted {path}"));
        }
        catch (Exception ex) { return Task.FromResult(Fail(nodeId, ex.Message)); }
    }

    private NodeExecutionResult Ok(string nodeId, string msg)   => new() { NodeId = nodeId, ModuleId = ModuleId, Status = "success", Message = msg };
    private NodeExecutionResult Fail(string nodeId, string msg) => new() { NodeId = nodeId, ModuleId = ModuleId, Status = "failed",  Message = msg };
}
