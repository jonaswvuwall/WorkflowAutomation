using WorkflowEngine.Modules;
using WorkflowEngine.Modules.Actions;
using WorkflowEngine.Modules.Conditions;
using WorkflowEngine.Modules.Events;
using WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient(string.Empty)
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
builder.Services.AddSingleton<JsonDataService>();

// ── Event modules (Singleton — own background listeners) ────────────────────
builder.Services.AddSingleton<IEventModule, FileWatcherEventModule>();
builder.Services.AddSingleton<IEventModule, ManualEventModule>();
builder.Services.AddSingleton<IEventModule, SchedulerEventModule>();
builder.Services.AddSingleton<IEventModule, CalendarEventModule>();

// ── Condition modules (Singleton — stateless evaluators) ─────────────────────
builder.Services.AddSingleton<IConditionModule, AlwaysTrueConditionModule>();
builder.Services.AddSingleton<IConditionModule, ValueEqualsConditionModule>();
builder.Services.AddSingleton<IConditionModule, ValueContainsConditionModule>();
builder.Services.AddSingleton<IConditionModule, TimeInRangeConditionModule>();

// ── Action modules (Transient — stateless) ───────────────────────────────────
builder.Services.AddTransient<IActionModule, CreateFileActionModule>();
builder.Services.AddTransient<IActionModule, DeleteFileActionModule>();
builder.Services.AddTransient<IActionModule, CopyFileActionModule>();
builder.Services.AddTransient<IActionModule, MoveFileActionModule>();
builder.Services.AddTransient<IActionModule, LogActionModule>();
builder.Services.AddTransient<IActionModule, FetchBearerTokenActionModule>();
builder.Services.AddTransient<IActionModule, HttpRequestActionModule>();
builder.Services.AddTransient<IActionModule, WaitActionModule>();

// ── Orchestration layer ──────────────────────────────────────────────────────
builder.Services.AddSingleton<ModuleRegistry>();
builder.Services.AddSingleton<WorkflowDispatcher>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<WorkflowDispatcher>());

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors();
app.MapControllers();

// Initialize data files on startup
var dataService = app.Services.GetRequiredService<JsonDataService>();
dataService.Initialize();

app.Run();
