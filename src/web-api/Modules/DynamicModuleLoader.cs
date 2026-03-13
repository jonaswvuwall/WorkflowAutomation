using WorkflowEngine.Models;
using WorkflowEngine.Modules.Dynamic;
using WorkflowEngine.Services;

namespace WorkflowEngine.Modules;

/// <summary>
/// Loads custom module definitions from CustomModulesService into the ModuleRegistry at startup
/// and provides methods for hot-loading/unloading after CRUD operations.
/// </summary>
public class DynamicModuleLoader(
    CustomModulesService customModulesService,
    ModuleRegistry registry,
    IHttpClientFactory httpFactory,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<DynamicModuleLoader>();

    public void LoadAll()
    {
        var defs = customModulesService.GetAll();
        foreach (var def in defs)
            Load(def);

        _logger.LogInformation("Loaded {Count} custom module(s)", defs.Count);
    }

    public void Load(CustomModuleDefinition def)
    {
        switch (def.ModuleType)
        {
            case "event":
                registry.AddDynamic(new DynamicEventModule(def));
                break;
            case "action":
                registry.AddDynamic(new DynamicActionModule(
                    def, httpFactory, loggerFactory.CreateLogger<DynamicActionModule>()));
                break;
            default:
                _logger.LogWarning("Unknown module type '{Type}' for custom module '{Id}'", def.ModuleType, def.Id);
                break;
        }
    }

    public void Unload(string moduleId)
        => registry.RemoveDynamic(moduleId);
}
