using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.ProjectionReplayAggregate.Events;

namespace ti8m.BeachBreak.Domain.ProjectionReplayAggregate;

/// <summary>
/// Aggregate tracking the lifecycle of a projection replay operation.
/// Event sourced to provide full audit trail of replay operations.
/// </summary>
public class ProjectionReplay : AggregateRoot
{
    public string ProjectionName { get; private set; } = null!;
    public ReplayStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public long TotalEvents { get; private set; }
    public long ProcessedEvents { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Guid InitiatedBy { get; private set; }
    public string Reason { get; private set; } = null!;

    public int ProgressPercentage => TotalEvents > 0
        ? (int)((ProcessedEvents * 100) / TotalEvents)
        : 0;

    private ProjectionReplay() { }

    public static ProjectionReplay Start(
        Guid id,
        string projectionName,
        Guid initiatedBy,
        string reason)
    {
        var replay = new ProjectionReplay();
        replay.RaiseEvent(new ProjectionReplayStarted(
            id,
            projectionName,
            DateTime.UtcNow,
            initiatedBy,
            reason));
        return replay;
    }

    public void SetTotalEvents(long totalEvents)
    {
        if (totalEvents > 0 && TotalEvents != totalEvents)
        {
            RaiseEvent(new ProjectionReplayTotalEventsSet(totalEvents, DateTime.UtcNow));
        }
    }

    public void UpdateProgress(ReplayStatus status, long processedEvents)
    {
        if (Status == ReplayStatus.Cancelled || Status == ReplayStatus.Completed || Status == ReplayStatus.Failed)
        {
            // Cannot update progress after terminal state
            return;
        }

        RaiseEvent(new ProjectionReplayProgressUpdated(
            status,
            processedEvents,
            DateTime.UtcNow));
    }

    public void Complete()
    {
        if (Status != ReplayStatus.Completed)
        {
            RaiseEvent(new ProjectionReplayCompleted(DateTime.UtcNow));
        }
    }

    public void Fail(string errorMessage)
    {
        if (Status != ReplayStatus.Failed)
        {
            RaiseEvent(new ProjectionReplayFailed(errorMessage, DateTime.UtcNow));
        }
    }

    public void Cancel(Guid cancelledBy)
    {
        if (Status != ReplayStatus.Cancelled && Status != ReplayStatus.Completed && Status != ReplayStatus.Failed)
        {
            RaiseEvent(new ProjectionReplayCancelled(cancelledBy, DateTime.UtcNow));
        }
    }

    // Apply methods for event sourcing
    private void Apply(ProjectionReplayStarted @event)
    {
        Id = @event.AggregateId;
        ProjectionName = @event.ProjectionName;
        StartedAt = @event.StartedAt;
        InitiatedBy = @event.InitiatedBy;
        Reason = @event.Reason;
        Status = ReplayStatus.Pending;
        ProcessedEvents = 0;
        TotalEvents = 0;
    }

    private void Apply(ProjectionReplayTotalEventsSet @event)
    {
        TotalEvents = @event.TotalEvents;
    }

    private void Apply(ProjectionReplayProgressUpdated @event)
    {
        Status = @event.Status;
        ProcessedEvents = @event.ProcessedEvents;
    }

    private void Apply(ProjectionReplayCompleted @event)
    {
        Status = ReplayStatus.Completed;
        CompletedAt = @event.CompletedAt;
    }

    private void Apply(ProjectionReplayFailed @event)
    {
        Status = ReplayStatus.Failed;
        ErrorMessage = @event.ErrorMessage;
        CompletedAt = @event.FailedAt;
    }

    private void Apply(ProjectionReplayCancelled @event)
    {
        Status = ReplayStatus.Cancelled;
        CompletedAt = @event.CancelledAt;
    }
}
