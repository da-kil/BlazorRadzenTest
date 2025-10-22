namespace ti8m.BeachBreak.Domain.ProjectionReplayAggregate;

/// <summary>
/// Status of a projection replay operation
/// </summary>
public enum ReplayStatus
{
    Pending,
    Validating,
    DeletingSnapshots,
    Replaying,
    Completed,
    Failed,
    Cancelled
}
