namespace ti8m.BeachBreak.Domain.ProjectionReplayAggregate;

/// <summary>
/// Status of a projection replay operation
/// </summary>
public enum ReplayStatus
{
    Pending = 0,
    Validating = 1,
    DeletingSnapshots = 2,
    Replaying = 3,
    Completed = 4,
    Failed = 5,
    Cancelled = 6
}
