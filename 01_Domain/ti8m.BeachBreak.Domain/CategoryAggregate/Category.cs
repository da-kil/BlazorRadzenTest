using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.CategoryAggregate.Events;

namespace ti8m.BeachBreak.Domain.CategoryAggregate;

public class Category : AggregateRoot
{
    public Translation Name { get; private set; }
    public Translation Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? LastModifiedDate { get; private set; }
    public int SortOrder { get; private set; }

    private Category() { }

    public Category(
        Guid id,
        Translation name,
        Translation description,
        int sortOrder)
    {
        RaiseEvent(new CategoryAdded(
            id,
            name,
            description,
            DateTime.UtcNow,
            DateTime.UtcNow,
            sortOrder));
    }

    public void Apply(CategoryAdded @event)
    {
        Id = @event.aggregateId;
        Name = @event.name;
        Description = @event.description;
        IsActive = true;
        CreatedDate = @event.createdDate;
        LastModifiedDate = @event.lastModifiedDate;
        SortOrder = @event.sortOrder;
    }
}
