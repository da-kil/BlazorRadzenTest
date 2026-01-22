using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.CategoryAggregate.Events;

namespace ti8m.BeachBreak.Domain.CategoryAggregate;

public partial class Category : AggregateRoot
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

    public void ChangeName(Translation newName)
    {
        if (newName != null && !newName.Equals(Name))
        {
            RaiseEvent(new CategoryNameChanged(newName, DateTime.UtcNow));
        }
    }

    public void ChangeDescription(Translation newDescription)
    {
        if (newDescription != null && !newDescription.Equals(Description))
        {
            RaiseEvent(new CategoryDescriptionChanged(newDescription, DateTime.UtcNow));
        }
    }

    public void ChangeSortOrder(int newSortOrder)
    {
        if (newSortOrder != SortOrder)
        {
            RaiseEvent(new CategorySortOrderChanged(newSortOrder, DateTime.UtcNow));
        }
    }

    public void Activate()
    {
        if (!IsActive)
        {
            RaiseEvent(new CategoryActivated(DateTime.UtcNow));
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            RaiseEvent(new CategoryDeactivated(DateTime.UtcNow));
        }
    }

    public void Apply(CategoryAdded @event)
    {
        Id = @event.AggregateId;
        Name = @event.Name;
        Description = @event.Description;
        IsActive = true;
        CreatedDate = @event.CreatedDate;
        LastModifiedDate = @event.LastModifiedDate;
        SortOrder = @event.SortOrder;
    }

    public void Apply(CategoryNameChanged @event)
    {
        Name = @event.Name;
        LastModifiedDate = @event.LastModifiedDate;
    }

    public void Apply(CategoryDescriptionChanged @event)
    {
        Description = @event.Description;
        LastModifiedDate = @event.LastModifiedDate;
    }

    public void Apply(CategorySortOrderChanged @event)
    {
        SortOrder = @event.SortOrder;
        LastModifiedDate = @event.LastModifiedDate;
    }

    public void Apply(CategoryActivated @event)
    {
        IsActive = true;
        LastModifiedDate = @event.LastModifiedDate;
    }

    public void Apply(CategoryDeactivated @event)
    {
        IsActive = false;
        LastModifiedDate = @event.LastModifiedDate;
    }
}
