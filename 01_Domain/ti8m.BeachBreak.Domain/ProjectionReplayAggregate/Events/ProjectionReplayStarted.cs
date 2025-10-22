using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.ProjectionReplayAggregate.Events;

public record ProjectionReplayStarted(
    Guid AggregateId,
    string ProjectionName,
    DateTime StartedAt,
    Guid InitiatedBy,
    string Reason) : IDomainEvent;
