namespace WorkflowEngine.Models;

public class TriggerContext
{
    public string TriggerType { get; init; } = string.Empty;
    public Dictionary<string, string> Fields { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public static TriggerContext FromFile(string triggerType, string fullPath)
    {
        var info = new FileInfo(fullPath);
        return new TriggerContext
        {
            TriggerType = triggerType,
            Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["filename"]  = info.Name,
                ["extension"] = info.Extension,
                ["filepath"]  = info.FullName,
                ["filesize"]  = info.Exists ? info.Length.ToString() : "0",
            }
        };
    }

    public static TriggerContext Manual() => new() { TriggerType = "manual" };
}
