using System.Text.Json;
using WorkflowEngine.Models;

namespace WorkflowEngine.Services;

public class JsonDataService
{
    private readonly string _dataDir;
    private readonly string _workflowsFile;
    private readonly string _runsFile;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonDataService(IWebHostEnvironment env)
    {
        _dataDir = Path.Combine(env.ContentRootPath, "data");
        _workflowsFile = Path.Combine(_dataDir, "workflows.json");
        _runsFile = Path.Combine(_dataDir, "runs.json");
    }

    public void Initialize()
    {
        Directory.CreateDirectory(_dataDir);

        if (!File.Exists(_workflowsFile))
            File.WriteAllText(_workflowsFile, "[]");

        if (!File.Exists(_runsFile))
            File.WriteAllText(_runsFile, "[]");
    }

    // Workflows
    public List<Workflow> GetWorkflows()
    {
        var json = File.ReadAllText(_workflowsFile);
        return JsonSerializer.Deserialize<List<Workflow>>(json, _jsonOptions) ?? [];
    }

    public Workflow? GetWorkflow(string id)
        => GetWorkflows().FirstOrDefault(w => w.Id == id);

    public Workflow AddWorkflow(Workflow workflow)
    {
        var workflows = GetWorkflows();
        workflow.Id = Guid.NewGuid().ToString();
        workflow.CreatedAt = DateTime.UtcNow;
        workflow.UpdatedAt = DateTime.UtcNow;
        workflows.Add(workflow);
        SaveWorkflows(workflows);
        return workflow;
    }

    public Workflow? UpdateWorkflow(string id, Workflow updated)
    {
        var workflows = GetWorkflows();
        var index = workflows.FindIndex(w => w.Id == id);
        if (index < 0) return null;

        updated.Id = id;
        updated.CreatedAt = workflows[index].CreatedAt;
        updated.UpdatedAt = DateTime.UtcNow;
        workflows[index] = updated;
        SaveWorkflows(workflows);
        return updated;
    }

    public bool DeleteWorkflow(string id)
    {
        var workflows = GetWorkflows();
        var removed = workflows.RemoveAll(w => w.Id == id);
        if (removed == 0) return false;
        SaveWorkflows(workflows);
        return true;
    }

    private void SaveWorkflows(List<Workflow> workflows)
        => File.WriteAllText(_workflowsFile, JsonSerializer.Serialize(workflows, _jsonOptions));

    // Runs
    public List<Run> GetRuns()
    {
        var json = File.ReadAllText(_runsFile);
        return JsonSerializer.Deserialize<List<Run>>(json, _jsonOptions) ?? [];
    }

    public Run? GetRun(string id)
        => GetRuns().FirstOrDefault(r => r.Id == id);

    public List<Run> GetRunsByWorkflow(string workflowId)
        => GetRuns().Where(r => r.WorkflowId == workflowId).ToList();

    public Run AddRun(Run run)
    {
        var runs = GetRuns();
        run.Id = Guid.NewGuid().ToString();
        runs.Add(run);
        SaveRuns(runs);
        return run;
    }

    private void SaveRuns(List<Run> runs)
        => File.WriteAllText(_runsFile, JsonSerializer.Serialize(runs, _jsonOptions));
}
