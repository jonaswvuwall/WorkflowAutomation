using System.Net.Http.Headers;
using System.Text;

namespace WorkflowEngine.Modules.Actions;

public sealed class HttpRequestActionModule(IHttpClientFactory httpClientFactory) : IActionModule
{
    public string ModuleId => "action.http_request";

    public ModuleManifest Manifest => new()
    {
        Id          = "action.http_request",
        Name        = "HTTP Request",
        Description = "Sends an HTTP request with optional payload and authorisation",
        Category    = "HTTP",
        Parameters  =
        [
            new ParameterSchema
            {
                Key      = "url",
                Label    = "URL",
                Type     = "text",
                Required = true,
                Default  = ""
            },
            new ParameterSchema
            {
                Key      = "method",
                Label    = "Method",
                Type     = "select",
                Required = true,
                Default  = "GET",
                Options  =
                [
                    new SelectOption { Value = "GET",    Label = "GET" },
                    new SelectOption { Value = "POST",   Label = "POST" },
                    new SelectOption { Value = "PUT",    Label = "PUT" },
                    new SelectOption { Value = "PATCH",  Label = "PATCH" },
                    new SelectOption { Value = "DELETE", Label = "DELETE" }
                ]
            },
            new ParameterSchema
            {
                Key      = "content_type",
                Label    = "Content-Type",
                Type     = "text",
                Required = false,
                Default  = "application/json"
            },
            new ParameterSchema
            {
                Key      = "body",
                Label    = "Request Body",
                Type     = "textarea",
                Required = false,
                Default  = ""
            },
            new ParameterSchema
            {
                Key      = "auth_type",
                Label    = "Authorisation",
                Type     = "select",
                Required = false,
                Default  = "none",
                Options  =
                [
                    new SelectOption { Value = "none",    Label = "None" },
                    new SelectOption { Value = "bearer",  Label = "Bearer Token" },
                    new SelectOption { Value = "basic",   Label = "Basic Auth" },
                    new SelectOption { Value = "api_key", Label = "API Key" }
                ]
            },
            new ParameterSchema
            {
                Key         = "bearer_token",
                Label       = "Bearer Token",
                Type        = "text",
                Required    = false,
                Default     = "",
                VisibleWhen = new VisibleWhen { Key = "auth_type", Value = "bearer" }
            },
            new ParameterSchema
            {
                Key         = "basic_username",
                Label       = "Username",
                Type        = "text",
                Required    = false,
                Default     = "",
                VisibleWhen = new VisibleWhen { Key = "auth_type", Value = "basic" }
            },
            new ParameterSchema
            {
                Key         = "basic_password",
                Label       = "Password",
                Type        = "text",
                Required    = false,
                Default     = "",
                VisibleWhen = new VisibleWhen { Key = "auth_type", Value = "basic" }
            },
            new ParameterSchema
            {
                Key         = "api_key_header",
                Label       = "API Key Header",
                Type        = "text",
                Required    = false,
                Default     = "X-Api-Key",
                VisibleWhen = new VisibleWhen { Key = "auth_type", Value = "api_key" }
            },
            new ParameterSchema
            {
                Key         = "api_key_value",
                Label       = "API Key Value",
                Type        = "text",
                Required    = false,
                Default     = "",
                VisibleWhen = new VisibleWhen { Key = "auth_type", Value = "api_key" }
            }
        ]
    };

    public async Task<ActionResult> ExecuteAsync(Dictionary<string, string> config, TriggerContext context)
    {
        var p = new ModuleParameters(config);
        try
        {
            var url         = p.Require("url");
            var method      = p.Get("method", "GET").ToUpperInvariant();
            var body        = p.Get("body", "");
            var contentType = p.Get("content_type", "application/json");
            var authType    = p.Get("auth_type", "none");

            var client  = httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(new HttpMethod(method), url);

            ApplyAuth(request, authType, p);

            if (!string.IsNullOrEmpty(body) && method is not "GET" and not "DELETE")
                request.Content = new StringContent(body, Encoding.UTF8, contentType);

            var resp   = await client.SendAsync(request);
            var status = (int)resp.StatusCode;
            return new ActionResult(resp.IsSuccessStatusCode, $"HTTP {status}");
        }
        catch (Exception ex)
        {
            return new ActionResult(false, ex.Message);
        }
    }

    private static void ApplyAuth(HttpRequestMessage request, string authType, ModuleParameters p)
    {
        switch (authType)
        {
            case "bearer":
                var token = p.Get("bearer_token", "");
                if (!string.IsNullOrWhiteSpace(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                break;

            case "basic":
                var username = p.Get("basic_username", "");
                var password = p.Get("basic_password", "");
                var encoded  = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
                break;

            case "api_key":
                var headerName = p.Get("api_key_header", "X-Api-Key");
                var headerVal  = p.Get("api_key_value", "");
                if (!string.IsNullOrWhiteSpace(headerName) && !string.IsNullOrWhiteSpace(headerVal))
                    request.Headers.TryAddWithoutValidation(headerName, headerVal);
                break;
        }
    }
}
