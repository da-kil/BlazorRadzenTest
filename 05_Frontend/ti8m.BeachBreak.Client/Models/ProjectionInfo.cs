namespace ti8m.BeachBreak.Client.Models;

public class ProjectionInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public long CurrentSnapshotCount { get; set; }
    public DateTime? LastRebuilt { get; set; }
    public bool IsRebuildable { get; set; }
}
