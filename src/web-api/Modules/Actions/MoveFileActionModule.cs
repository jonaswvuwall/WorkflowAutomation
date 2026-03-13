namespace WorkflowEngine.Modules.Actions;

public sealed class MoveFileActionModule : IActionModule
{
    public string ModuleId => "action.move_file";

    public ModuleManifest Manifest => new()
    {
        Id          = "action.move_file",
        Name        = "Move File",
        Description = "Moves a file from source to destination",
        Category    = "File System",
        Parameters  =
        [
            new ParameterSchema { Key = "source",      Label = "Source Path",      Type = "text", Required = true, Default = "" },
            new ParameterSchema { Key = "destination", Label = "Destination Path", Type = "text", Required = true, Default = "" }
        ]
    };

    public Task<ActionResult> ExecuteAsync(Dictionary<string, string> config, TriggerContext context)
    {
        var p = new ModuleParameters(config);
        try
        {
            var src = p.Require("source");
            var dst = p.Require("destination");
            var dir = Path.GetDirectoryName(dst);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.Move(src, dst, overwrite: true);
            return Task.FromResult(new ActionResult(true, $"Moved {src} → {dst}"));
        }
        catch (Exception ex) { return Task.FromResult(new ActionResult(false, ex.Message)); }
    }
}
