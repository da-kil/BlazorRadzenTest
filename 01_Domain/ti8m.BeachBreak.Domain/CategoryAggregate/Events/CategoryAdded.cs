using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.CategoryAggregate.Events;

public record CategoryAdded(
    Guid aggregateId,
    Translation name,
    Translation description,
    DateTime createdDate,
    DateTime lastModifiedDate,
    int sortOrder) : IDomainEvent;
