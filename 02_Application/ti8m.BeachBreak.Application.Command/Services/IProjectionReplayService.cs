using ti8m.BeachBreak.Application.Command.Commands;

namespace ti8m.BeachBreak.Application.Command.Services;

/// <summary>
/// High-level service for orchestrating projection replay operations
/// </summary>
public interface IProjectionReplayService
{
    /// <summary>
    /// Start a projection replay operation
    /// Runs in background and returns replay ID immediately
    /// </summary>
    Task<Result<Guid>> StartReplayAsync(
        string projectionName,
        Guid initiatedBy,
        string reason,
        CancellationToken ct = default);

    /// <summary>
    /// Cancel an in-progress replay operation
    /// </summary>
    Task<Result> CancelReplayAsync(Guid replayId, Guid cancelledBy, CancellationToken ct = default);
}
