using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.ProjectionReplayAggregate.Events;

public record ProjectionReplayCancelled(
    Guid CancelledBy,
    DateTime CancelledAt) : IDomainEvent;
