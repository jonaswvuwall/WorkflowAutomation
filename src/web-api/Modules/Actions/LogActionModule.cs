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
            new ParameterSchema { Key = "message", Label = "Message", Type = "textarea", Required = false }
        ]
    };

    public Task<NodeExecutionResult> ExecuteAsync(
        string nodeId, Dictionary<string, string> config, TriggerContext context)
    {
        var p       = new ModuleParameters(config);
        var message = p.Get("message", "(no message)");
        logger.LogInformation("[workflow log] {Message}", message);
        return Task.FromResult(new NodeExecutionResult
        {
            NodeId   = nodeId,
            ModuleId = ModuleId,
            Status   = "success",
            Message  = message
        });
    }
}
