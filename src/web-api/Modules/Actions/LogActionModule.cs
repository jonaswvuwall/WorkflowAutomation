namespace WorkflowEngine.Modules.Actions;

public sealed class LogActionModule(ILogger<LogActionModule> logger) : IActionModule
{
    public string ModuleId => "action.log";

    public ModuleManifest Manifest => new()
    {
        Id          = "action.log",
        Name        = "Log Message",
        Description = "Logs a message to the application log",
        Category    = "General",
        Parameters  =
        [
            new ParameterSchema { Key = "message", Label = "Message", Type = "textarea", Required = false, Default = "{{eventName}} triggered" }
        ]
    };

    public Task<ActionResult> ExecuteAsync(Dictionary<string, string> config, TriggerContext context)
    {
        var p       = new ModuleParameters(config);
        var message = p.Get("message", "(no message)");
        logger.LogInformation("[workflow log] {Message}", message);
        return Task.FromResult(new ActionResult(true, message));
    }
}
