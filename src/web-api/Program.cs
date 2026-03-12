using WorkflowEngine.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<JsonDataService>();
builder.Services.AddSingleton<ActionExecutor>();
builder.Services.AddHostedService<FileWatcherService>();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors();
app.MapControllers();

// Ensure data directory and files exist on startup
var dataService = app.Services.GetRequiredService<JsonDataService>();
dataService.Initialize();

app.Run();
