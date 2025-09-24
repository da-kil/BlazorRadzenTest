using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.CategoryAggregate.Events;

public record CategoryDescriptionChanged(Translation Description, DateTime LastModifiedDate) : IDomainEvent;