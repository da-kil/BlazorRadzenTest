using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.ProjectionReplayAggregate.Events;

public record ProjectionReplayCompleted(DateTime CompletedAt) : IDomainEvent;
