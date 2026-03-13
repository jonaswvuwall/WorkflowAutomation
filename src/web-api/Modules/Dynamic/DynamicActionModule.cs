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

    public Task<ActionResult> ExecuteAsync(Dictionary<string, string> config, TriggerContext context)
    {
        return def.BaseType switch
        {
            "script"       => RunScript(config),
            "http_request" => RunHttp(config),
            _ => Task.FromResult(new ActionResult(false, $"Unknown base type: {def.BaseType}"))
        };
    }

    // ── Script execution ─────────────────────────────────────────────────────

    private async Task<ActionResult> RunScript(Dictionary<string, string> config)
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
                return new ActionResult(false, string.IsNullOrWhiteSpace(stderr) ? $"Exit code {process.ExitCode}" : stderr.Trim());
            }

            return new ActionResult(true, stdout.Trim());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Script module {Id} threw", def.Id);
            return new ActionResult(false, ex.Message);
        }
    }

    // ── HTTP request ──────────────────────────────────────────────────────────

    private async Task<ActionResult> RunHttp(Dictionary<string, string> config)
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
            return new ActionResult(resp.IsSuccessStatusCode, $"HTTP {status}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HTTP module {Id} threw", def.Id);
            return new ActionResult(false, ex.Message);
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
}
