using ti8m.BeachBreak.Domain.ProjectionReplayAggregate;
using ti8m.BeachBreak.Domain.ProjectionReplayAggregate.Events;

namespace ti8m.BeachBreak.Application.Query.Projections;

public class ProjectionReplayReadModel
{
    public Guid Id { get; set; }
    public string ProjectionName { get; set; } = null!;
    public ReplayStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long TotalEvents { get; set; }
    public long ProcessedEvents { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid InitiatedBy { get; set; }
    public string Reason { get; set; } = null!;

    public int ProgressPercentage => TotalEvents > 0
        ? (int)((ProcessedEvents * 100) / TotalEvents)
        : 0;

    public void Apply(ProjectionReplayStarted @event)
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

    public void Apply(ProjectionReplayTotalEventsSet @event)
    {
        TotalEvents = @event.TotalEvents;
    }

    public void Apply(ProjectionReplayProgressUpdated @event)
    {
        Status = @event.Status;
        ProcessedEvents = @event.ProcessedEvents;
    }

    public void Apply(ProjectionReplayCompleted @event)
    {
        Status = ReplayStatus.Completed;
        CompletedAt = @event.CompletedAt;
    }

    public void Apply(ProjectionReplayFailed @event)
    {
        Status = ReplayStatus.Failed;
        ErrorMessage = @event.ErrorMessage;
        CompletedAt = @event.FailedAt;
    }

    public void Apply(ProjectionReplayCancelled @event)
    {
        Status = ReplayStatus.Cancelled;
        CompletedAt = @event.CancelledAt;
    }
}
