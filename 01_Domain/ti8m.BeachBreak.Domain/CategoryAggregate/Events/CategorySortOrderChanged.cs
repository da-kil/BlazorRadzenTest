using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.CategoryAggregate.Events;

public record CategorySortOrderChanged(int SortOrder, DateTime LastModifiedDate) : IDomainEvent;