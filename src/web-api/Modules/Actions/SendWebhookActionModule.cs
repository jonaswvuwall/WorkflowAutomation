using System.Text;

namespace WorkflowEngine.Modules.Actions;

public sealed class SendWebhookActionModule(IHttpClientFactory httpClientFactory) : IActionModule
{
    public string ModuleId => "action.send_webhook";

    public ModuleManifest Manifest => new()
    {
        Id          = "action.send_webhook",
        Name        = "Send Webhook",
        Description = "Sends a POST request to an HTTP endpoint",
        Category    = "HTTP",
        Parameters  =
        [
            new ParameterSchema { Key = "url",  Label = "URL",         Type = "text",     Required = true,  Default = "" },
            new ParameterSchema { Key = "body", Label = "Request Body", Type = "textarea", Required = false, Default = "{\n  \"event\": \"{{eventName}}\"\n}" }
        ]
    };

    public async Task<ActionResult> ExecuteAsync(Dictionary<string, string> config, TriggerContext context)
    {
        var p = new ModuleParameters(config);
        try
        {
            var url    = p.Require("url");
            var body   = p.Get("body", "{}");
            var client = httpClientFactory.CreateClient();
            var resp   = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
            var status = (int)resp.StatusCode;
            return new ActionResult(resp.IsSuccessStatusCode, $"HTTP {status}");
        }
        catch (Exception ex)
        {
            return new ActionResult(false, ex.Message);
        }
    }
}
