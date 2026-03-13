using WorkflowEngine.Modules;
using WorkflowEngine.Modules.Actions;
using WorkflowEngine.Modules.Events;
using WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<JsonDataService>();

// ── Event modules (Singleton — own background listeners) ────────────────────
builder.Services.AddSingleton<IEventModule, FileWatcherEventModule>();
builder.Services.AddSingleton<IEventModule, ManualEventModule>();

// ── Action modules (Transient — stateless) ───────────────────────────────────
builder.Services.AddTransient<IActionModule, CreateFileActionModule>();
builder.Services.AddTransient<IActionModule, DeleteFileActionModule>();
builder.Services.AddTransient<IActionModule, CopyFileActionModule>();
builder.Services.AddTransient<IActionModule, MoveFileActionModule>();
builder.Services.AddTransient<IActionModule, LogActionModule>();
builder.Services.AddTransient<IActionModule, SendWebhookActionModule>();

// ── Orchestration layer ──────────────────────────────────────────────────────
builder.Services.AddSingleton<ModuleRegistry>();
builder.Services.AddSingleton<CustomModulesService>();
builder.Services.AddSingleton<DynamicModuleLoader>();
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

// Load custom module definitions into the registry
var dynamicLoader = app.Services.GetRequiredService<DynamicModuleLoader>();
dynamicLoader.LoadAll();

app.Run();
