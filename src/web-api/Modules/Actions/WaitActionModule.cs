namespace WorkflowEngine.Modules.Actions;

public sealed class WaitActionModule : IActionModule
{
    public string ModuleId => "action.wait";

    public ModuleManifest Manifest => new()
    {
        Id          = "action.wait",
        Name        = "Wait",
        Description = "Pauses execution for a given number of milliseconds",
        Category    = "Control",
        Parameters  =
        [
            new ParameterSchema
            {
                Key      = "duration_ms",
                Label    = "Duration (ms)",
                Type     = "number",
                Required = true,
                Default  = "1000"
            }
        ]
    };

    public async Task<ActionResult> ExecuteAsync(Dictionary<string, string> config, TriggerContext context)
    {
        var p  = new ModuleParameters(config);
        var ms = int.TryParse(p.Get("duration_ms", "1000"), out var v) ? v : 1000;
        await Task.Delay(ms);
        return new ActionResult(true, $"Waited {ms} ms");
    }
}
