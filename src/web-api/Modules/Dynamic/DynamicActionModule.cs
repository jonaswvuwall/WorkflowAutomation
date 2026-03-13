using System.Diagnostics;
using System.Text;
using WorkflowEngine.Models;

namespace WorkflowEngine.Modules.Dynamic;

public sealed class DynamicActionModule(
    CustomModuleDefinition def,
    IHttpClientFactory httpFactory,
    ILogger<DynamicActionModule> logger) : IActionModule
{
    public string ModuleId => def.Id;

    public ModuleManifest Manifest => new()
    {
        Id          = def.Id,
        Name        = def.Name,
        Description = def.Description,
        Category    = def.Category,
        Parameters  = def.Parameters
    };

    public Task<NodeExecutionResult> ExecuteAsync(
        string nodeId, Dictionary<string, string> config, TriggerContext context)
    {
        return def.BaseType switch
        {
            "script"       => RunScript(nodeId, config),
            "http_request" => RunHttp(nodeId, config),
            _ => Task.FromResult(Fail(nodeId, $"Unknown base type: {def.BaseType}"))
        };
    }

    // ── Script execution ─────────────────────────────────────────────────────

    private async Task<NodeExecutionResult> RunScript(string nodeId, Dictionary<string, string> config)
    {
        var script = Substitute(def.ScriptContent ?? "", config);
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName               = "powershell.exe",
                Arguments              = $"-NonInteractive -NoProfile -Command \"{script.Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            using var process = Process.Start(psi)!;
            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                logger.LogWarning("Script module {Id} exited {Code}: {Err}", def.Id, process.ExitCode, stderr);
                return Fail(nodeId, string.IsNullOrWhiteSpace(stderr) ? $"Exit code {process.ExitCode}" : stderr.Trim());
            }

            return Ok(nodeId, stdout.Trim());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Script module {Id} threw", def.Id);
            return Fail(nodeId, ex.Message);
        }
    }

    // ── HTTP request ──────────────────────────────────────────────────────────

    private async Task<NodeExecutionResult> RunHttp(string nodeId, Dictionary<string, string> config)
    {
        var url    = Substitute(def.HttpUrl  ?? "", config);
        var body   = Substitute(def.HttpBody ?? "{}", config);
        var method = def.HttpMethod?.ToUpperInvariant() ?? "POST";

        try
        {
            var client  = httpFactory.CreateClient();
            var request = new HttpRequestMessage(new HttpMethod(method), url);
            if (method is "POST" or "PUT" or "PATCH")
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var resp   = await client.SendAsync(request);
            var status = (int)resp.StatusCode;
            return resp.IsSuccessStatusCode
                ? Ok(nodeId, $"HTTP {status}")
                : Fail(nodeId, $"HTTP {status}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HTTP module {Id} threw", def.Id);
            return Fail(nodeId, ex.Message);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Substitutes {{key}} with config values.</summary>
    private static string Substitute(string template, Dictionary<string, string> config)
    {
        foreach (var (key, val) in config)
            template = template.Replace($"{{{{{key}}}}}", val, StringComparison.OrdinalIgnoreCase);
        return template;
    }

    private NodeExecutionResult Ok(string nodeId, string msg)
        => new() { NodeId = nodeId, ModuleId = ModuleId, Status = "success", Message = msg };

    private NodeExecutionResult Fail(string nodeId, string msg)
        => new() { NodeId = nodeId, ModuleId = ModuleId, Status = "failed", Message = msg };
}
