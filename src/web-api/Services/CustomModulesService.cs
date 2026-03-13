using System.Text.Json;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public class CustomModulesService
{
    private readonly string _file;
    private readonly JsonSerializerOptions _json = new()
    {
        WriteIndented        = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CustomModulesService(IWebHostEnvironment env)
    {
        _file = Path.Combine(env.ContentRootPath, "data", "custom-modules.json");
    }

    public void EnsureFile()
    {
        if (!File.Exists(_file))
            File.WriteAllText(_file, "[]");
    }

    public List<CustomModuleDefinition> GetAll()
    {
        EnsureFile();
        return JsonSerializer.Deserialize<List<CustomModuleDefinition>>(File.ReadAllText(_file), _json) ?? [];
    }

    public CustomModuleDefinition Add(CustomModuleDefinition def)
    {
        var all = GetAll();
        def.Id = $"custom.{Guid.NewGuid():N}";
        all.Add(def);
        Save(all);
        return def;
    }

    public CustomModuleDefinition? Update(string id, CustomModuleDefinition def)
    {
        var all   = GetAll();
        var index = all.FindIndex(d => d.Id == id);
        if (index < 0) return null;
        def.Id    = id;
        all[index] = def;
        Save(all);
        return def;
    }

    public bool Delete(string id)
    {
        var all     = GetAll();
        var removed = all.RemoveAll(d => d.Id == id);
        if (removed == 0) return false;
        Save(all);
        return true;
    }

    private void Save(List<CustomModuleDefinition> all)
        => File.WriteAllText(_file, JsonSerializer.Serialize(all, _json));
}
