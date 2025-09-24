using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.CategoryAggregate.Events;

public record CategoryActivated(DateTime LastModifiedDate) : IDomainEvent;