using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WorkflowEngine.Modules.Actions;

public sealed class FetchBearerTokenActionModule(IHttpClientFactory httpClientFactory) : IActionModule
{
    public string ModuleId => "action.fetch_bearer_token";

    public ModuleManifest Manifest => new()
    {
        Id          = "action.fetch_bearer_token",
        Name        = "Fetch Bearer Token",
        Description = "Authenticates against an API and stores the returned bearer token in context for use by later steps via {{key}}",
        Category    = "HTTP",
        Parameters  =
        [
            new ParameterSchema
            {
                Key      = "url",
                Label    = "Token URL",
                Type     = "text",
                Required = true,
                Default  = ""
            },
            new ParameterSchema
            {
                Key      = "grant_type",
                Label    = "Grant Type",
                Type     = "select",
                Required = true,
                Default  = "client_credentials",
                Options  =
                [
                    new SelectOption { Value = "client_credentials", Label = "Client Credentials" },
                    new SelectOption { Value = "password",           Label = "Password"            },
                    new SelectOption { Value = "custom_body",        Label = "Custom Body"         }
                ]
            },
            new ParameterSchema
            {
                Key         = "client_id",
                Label       = "Client ID",
                Type        = "text",
                Required    = false,
                Default     = "",
                VisibleWhen = new VisibleWhen { Key = "grant_type", Value = "client_credentials" }
            },
            new ParameterSchema
            {
                Key         = "client_secret",
                Label       = "Client Secret",
                Type        = "text",
                Required    = false,
                Default     = "",
                VisibleWhen = new VisibleWhen { Key = "grant_type", Value = "client_credentials" }
            },
            new ParameterSchema
            {
                Key         = "username",
                Label       = "Username",
                Type        = "text",
                Required    = false,
                Default     = "",
                VisibleWhen = new VisibleWhen { Key = "grant_type", Value = "password" }
            },
            new ParameterSchema
            {
                Key         = "password",
                Label       = "Password",
                Type        = "text",
                Required    = false,
                Default     = "",
                VisibleWhen = new VisibleWhen { Key = "grant_type", Value = "password" }
            },
            new ParameterSchema
            {
                Key         = "custom_body",
                Label       = "Request Body",
                Type        = "textarea",
                Required    = false,
                Default     = "",
                VisibleWhen = new VisibleWhen { Key = "grant_type", Value = "custom_body" }
            },
            new ParameterSchema
            {
                Key      = "content_type",
                Label    = "Content-Type",
                Type     = "select",
                Required = false,
                Default  = "application/x-www-form-urlencoded",
                Options  =
                [
                    new SelectOption { Value = "application/x-www-form-urlencoded", Label = "Form URL-encoded" },
                    new SelectOption { Value = "application/json",                  Label = "JSON"             }
                ]
            },
            new ParameterSchema
            {
                Key      = "token_field",
                Label    = "Token Field (JSON key in response)",
                Type     = "text",
                Required = false,
                Default  = "access_token"
            },
            new ParameterSchema
            {
                Key      = "output_key",
                Label    = "Output Key (used as {{key}} in later steps)",
                Type     = "text",
                Required = false,
                Default  = "bearerToken"
            }
        ]
    };

    public async Task<ActionResult> ExecuteAsync(Dictionary<string, string> config, TriggerContext context)
    {
        var p = new ModuleParameters(config);
        try
        {
            var url         = p.Require("url");
            var grantType   = p.Get("grant_type",   "client_credentials");
            var contentType = p.Get("content_type", "application/x-www-form-urlencoded");
            var tokenField  = p.Get("token_field",  "access_token");
            var outputKey   = p.Get("output_key",   "bearerToken");

            var body = BuildBody(grantType, contentType, p);

            var client  = httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body, Encoding.UTF8, contentType)
            };

            var resp = await client.SendAsync(request);
            var json = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                return new ActionResult(false, $"Auth request failed: HTTP {(int)resp.StatusCode} — {json}");

            var token = ExtractToken(json, tokenField);
            if (token is null)
                return new ActionResult(false, $"Token field '{tokenField}' not found in response");

            return new ActionResult(
                true,
                $"Token stored as {{{{ {outputKey} }}}}",
                OutputData: new Dictionary<string, string> { [outputKey] = token });
        }
        catch (Exception ex)
        {
            return new ActionResult(false, ex.Message);
        }
    }

    private static string BuildBody(string grantType, string contentType, ModuleParameters p)
    {
        if (grantType == "custom_body")
            return p.Get("custom_body", "");

        var fields = grantType == "client_credentials"
            ? new Dictionary<string, string>
            {
                ["grant_type"]    = "client_credentials",
                ["client_id"]     = p.Get("client_id",     ""),
                ["client_secret"] = p.Get("client_secret", "")
            }
            : new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"]   = p.Get("username", ""),
                ["password"]   = p.Get("password", "")
            };

        if (contentType == "application/json")
            return JsonSerializer.Serialize(fields);

        // application/x-www-form-urlencoded
        return string.Join("&", fields.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
    }

    private static string? ExtractToken(string json, string field)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(field, out var prop))
                return prop.GetString();

            // Case-insensitive fallback
            foreach (var element in doc.RootElement.EnumerateObject())
                if (string.Equals(element.Name, field, StringComparison.OrdinalIgnoreCase))
                    return element.Value.GetString();
        }
        catch { /* invalid JSON — fall through */ }
        return null;
    }
}
