using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.CategoryAggregate.Events;

public record CategoryAdded(
    Guid AggregateId,
    Translation Name,
    Translation Description,
    DateTime CreatedDate,
    DateTime LastModifiedDate,
    int SortOrder) : IDomainEvent;
