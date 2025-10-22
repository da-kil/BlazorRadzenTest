using System.Text.Json.Serialization;

namespace ti8m.BeachBreak.Application.Query.Models;

/// <summary>
/// Information about a rebuildable projection
/// </summary>
public record ProjectionInfo
{
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;

    [JsonIgnore]
    public Type DocumentType { get; init; } = null!;

    public string TableName { get; init; } = null!;
    public long CurrentSnapshotCount { get; init; }
    public DateTime? LastRebuilt { get; init; }
    public bool IsRebuildable { get; init; } = true;
}
