using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.ProjectionReplayAggregate.Events;

public record ProjectionReplayProgressUpdated(
    ReplayStatus Status,
    long ProcessedEvents,
    DateTime Timestamp) : IDomainEvent;
