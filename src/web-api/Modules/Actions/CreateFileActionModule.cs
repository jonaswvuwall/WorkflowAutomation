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
            new ParameterSchema { Key = "path",    Label = "File Path", Type = "text",     Required = true,  Default = "" },
            new ParameterSchema { Key = "content", Label = "Content",   Type = "textarea", Required = false, Default = "" }
        ]
    };

    public async Task<ActionResult> ExecuteAsync(Dictionary<string, string> config, TriggerContext context)
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
            return new ActionResult(true, $"Created {path}");
        }
        catch (Exception ex) { return new ActionResult(false, ex.Message); }
    }
}
