using System.Net.Http;
using System.Text;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public sealed class ActionExecutor(IHttpClientFactory httpClientFactory, ILogger<ActionExecutor> logger)
{
    public async Task<ActionResult> ExecuteAsync(WorkflowAction action)
    {
        var p = new Parameters(action.Parameters);
        try
        {
            return action.Type switch
            {
                "create_file"  => await CreateFile(p),
                "copy_file"    => CopyFile(p),
                "move_file"    => MoveFile(p),
                "delete_file"  => DeleteFile(p),
                "log"          => Log(action.Type, p),
                "send_webhook" => await SendWebhook(action.Type, p),
                _ => new ActionResult { Type = action.Type, Status = "failed", Message = $"Unknown action type '{action.Type}'" }
            };
        }
        catch (Exception ex)
        {
            return new ActionResult { Type = action.Type, Status = "failed", Message = ex.Message };
        }
    }

    private static async Task<ActionResult> CreateFile(Parameters p)
    {
        var path = p.Require("path");
        var content = p.Get("content", string.Empty);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(path, content);
        return new ActionResult { Type = "create_file", Status = "success", Message = $"Created {path}" };
    }

    private static ActionResult CopyFile(Parameters p)
    {
        var src = p.Require("source");
        var dst = p.Require("destination");
        var dir = Path.GetDirectoryName(dst);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.Copy(src, dst, overwrite: true);
        return new ActionResult { Type = "copy_file", Status = "success", Message = $"Copied {src} → {dst}" };
    }

    private static ActionResult MoveFile(Parameters p)
    {
        var src = p.Require("source");
        var dst = p.Require("destination");
        var dir = Path.GetDirectoryName(dst);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.Move(src, dst, overwrite: true);
        return new ActionResult { Type = "move_file", Status = "success", Message = $"Moved {src} → {dst}" };
    }

    private static ActionResult DeleteFile(Parameters p)
    {
        var path = p.Require("path");
        if (File.Exists(path))
            File.Delete(path);
        return new ActionResult { Type = "delete_file", Status = "success", Message = $"Deleted {path}" };
    }

    private ActionResult Log(string type, Parameters p)
    {
        var message = p.Get("message", "(no message)");
        logger.LogInformation("[workflow log] {Message}", message);
        return new ActionResult { Type = type, Status = "success", Message = message };
    }

    private async Task<ActionResult> SendWebhook(string type, Parameters p)
    {
        var url = p.Require("url");
        var body = p.Get("body", "{}");
        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
            return new ActionResult { Type = type, Status = "failed", Message = $"HTTP {(int)response.StatusCode}" };
        return new ActionResult { Type = type, Status = "success", Message = $"HTTP {(int)response.StatusCode}" };
    }

    private sealed class Parameters(Dictionary<string, string> raw)
    {
        private readonly Dictionary<string, string> _dict =
            new(raw, StringComparer.OrdinalIgnoreCase);

        public string Require(string key) =>
            _dict.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v)
                ? v
                : throw new InvalidOperationException($"Missing required parameter '{key}'");

        public string Get(string key, string defaultValue) =>
            _dict.TryGetValue(key, out var v) ? v : defaultValue;
    }
}
