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
            new ParameterSchema { Key = "path", Label = "File Path", Type = "text", Required = true, Default = "" }
        ]
    };

    public Task<ActionResult> ExecuteAsync(Dictionary<string, string> config, TriggerContext context)
    {
        var p = new ModuleParameters(config);
        try
        {
            var path = p.Require("path");
            if (File.Exists(path))
                File.Delete(path);
            return Task.FromResult(new ActionResult(true, $"Deleted {path}"));
        }
        catch (Exception ex) { return Task.FromResult(new ActionResult(false, ex.Message)); }
    }
}
