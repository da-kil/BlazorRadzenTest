using ti8m.BeachBreak.Domain.CategoryAggregate.Events;

namespace ti8m.BeachBreak.Application.Query.Projections;

public class CategoryReadModel
{
    public Guid Id { get; set; }
    public TranslationReadModel Name { get; set; }
    public TranslationReadModel Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int SortOrder { get; set; }

    public void Apply(CategoryAdded @event)
    {
        Id = @event.AggregateId;
        Name = new TranslationReadModel(@event.Name.English, @event.Name.German);
        Description = new TranslationReadModel(@event.Description.English, @event.Description.German);
        IsActive = true;
        CreatedDate = @event.CreatedDate;
        LastModifiedDate = @event.LastModifiedDate;
        SortOrder = @event.SortOrder;
    }

    public void Apply(CategoryNameChanged @event)
    {
        Name = new TranslationReadModel(@event.Name.English, @event.Name.German);
        LastModifiedDate = @event.LastModifiedDate;
    }

    public void Apply(CategoryDescriptionChanged @event)
    {
        Description = new TranslationReadModel(@event.Description.English, @event.Description.German);
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
