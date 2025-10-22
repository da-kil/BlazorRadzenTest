namespace ti8m.BeachBreak.Infrastructure.Marten.Replay;

/// <summary>
/// Service for rebuilding Marten projections from event store
/// </summary>
public interface IProjectionRebuilder
{
    /// <summary>
    /// Count total events that will be replayed for a projection
    /// </summary>
    Task<long> CountEventsForProjectionAsync(string projectionName, CancellationToken ct = default);

    /// <summary>
    /// Delete all snapshots for a projection
    /// </summary>
    Task DeleteSnapshotsAsync(string projectionName, CancellationToken ct = default);

    /// <summary>
    /// Rebuild projection from events with progress reporting
    /// </summary>
    Task RebuildProjectionAsync(
        string projectionName,
        IProgress<long>? progress = null,
        CancellationToken ct = default);

    /// <summary>
    /// Validate that projection can be rebuilt
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateProjectionAsync(string projectionName, CancellationToken ct = default);
}
