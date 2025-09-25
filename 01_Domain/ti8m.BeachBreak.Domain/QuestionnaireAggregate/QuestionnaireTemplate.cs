using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireAggregate.Events;

namespace ti8m.BeachBreak.Domain.QuestionnaireAggregate;

public class QuestionnaireTemplate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;

    public TemplateStatus Status { get; private set; } = TemplateStatus.Draft;
    public DateTime? PublishedDate { get; private set; }
    public DateTime? LastPublishedDate { get; private set; }
    public string PublishedBy { get; private set; } = string.Empty;

    public List<QuestionSection> Sections { get; private set; } = new();
    public QuestionnaireSettings Settings { get; private set; } = new();

    public DateTime CreatedDate { get; private set; }
    public DateTime? LastModifiedDate { get; private set; }

    private QuestionnaireTemplate() { }

    public QuestionnaireTemplate(
        Guid id,
        string name,
        string description,
        string category,
        List<QuestionSection>? sections = null,
        QuestionnaireSettings? settings = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        RaiseEvent(new QuestionnaireTemplateCreated(
            id,
            name,
            description ?? string.Empty,
            category ?? string.Empty,
            sections ?? new(),
            settings ?? new(),
            DateTime.UtcNow));
    }

    public bool CanBeAssignedToEmployee() => Status == TemplateStatus.Published;

    public bool CanBeEdited() => Status == TemplateStatus.Draft;

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        if (!string.Equals(Name, name, StringComparison.Ordinal))
        {
            RaiseEvent(new QuestionnaireTemplateNameChanged(Id, name, DateTime.UtcNow));
        }
    }

    public void ChangeDescription(string description)
    {
        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        var newDescription = description ?? string.Empty;
        if (!string.Equals(Description, newDescription, StringComparison.Ordinal))
        {
            RaiseEvent(new QuestionnaireTemplateDescriptionChanged(Id, newDescription, DateTime.UtcNow));
        }
    }

    public void ChangeCategory(string category)
    {
        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        var newCategory = category ?? string.Empty;
        if (!string.Equals(Category, newCategory, StringComparison.Ordinal))
        {
            RaiseEvent(new QuestionnaireTemplateCategoryChanged(Id, newCategory, DateTime.UtcNow));
        }
    }

    public void UpdateSections(List<QuestionSection> sections)
    {
        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        RaiseEvent(new QuestionnaireTemplateSectionsChanged(Id, sections ?? new(), DateTime.UtcNow));
    }

    public void UpdateSettings(QuestionnaireSettings settings)
    {
        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        RaiseEvent(new QuestionnaireTemplateSettingsChanged(Id, settings ?? new(), DateTime.UtcNow));
    }

    public void Publish(string publishedBy)
    {
        if (string.IsNullOrWhiteSpace(publishedBy))
            throw new ArgumentException("Publisher name is required", nameof(publishedBy));

        if (Status == TemplateStatus.Archived)
            throw new InvalidOperationException("Cannot publish an archived template");

        if (Status == TemplateStatus.Published)
            throw new InvalidOperationException("Template is already published");

        var now = DateTime.UtcNow;
        var publishedDate = PublishedDate ?? now;

        RaiseEvent(new QuestionnaireTemplatePublished(Id, publishedBy, publishedDate, now));
    }

    public void UnpublishToDraft()
    {
        if (Status != TemplateStatus.Published)
            throw new InvalidOperationException("Only published templates can be unpublished to draft");

        RaiseEvent(new QuestionnaireTemplateUnpublishedToDraft(Id, DateTime.UtcNow));
    }

    public void Archive()
    {
        if (Status == TemplateStatus.Archived)
            throw new InvalidOperationException("Template is already archived");

        RaiseEvent(new QuestionnaireTemplateArchived(Id, DateTime.UtcNow));
    }

    public void RestoreFromArchive()
    {
        if (Status != TemplateStatus.Archived)
            throw new InvalidOperationException("Only archived templates can be restored");

        RaiseEvent(new QuestionnaireTemplateRestoredFromArchive(Id, DateTime.UtcNow));
    }

    // Apply methods for event sourcing
    public void Apply(QuestionnaireTemplateCreated @event)
    {
        Id = @event.AggregateId;
        Name = @event.Name;
        Description = @event.Description;
        Category = @event.Category;
        Sections = @event.Sections;
        Settings = @event.Settings;
        Status = TemplateStatus.Draft;
        CreatedDate = @event.CreatedDate;
        LastModifiedDate = @event.CreatedDate;
    }

    public void Apply(QuestionnaireTemplateNameChanged @event)
    {
        Name = @event.Name;
        LastModifiedDate = @event.ModifiedDate;
    }

    public void Apply(QuestionnaireTemplateDescriptionChanged @event)
    {
        Description = @event.Description;
        LastModifiedDate = @event.ModifiedDate;
    }

    public void Apply(QuestionnaireTemplateCategoryChanged @event)
    {
        Category = @event.Category;
        LastModifiedDate = @event.ModifiedDate;
    }

    public void Apply(QuestionnaireTemplateSectionsChanged @event)
    {
        Sections = @event.Sections;
        LastModifiedDate = @event.ModifiedDate;
    }

    public void Apply(QuestionnaireTemplateSettingsChanged @event)
    {
        Settings = @event.Settings;
        LastModifiedDate = @event.ModifiedDate;
    }

    public void Apply(QuestionnaireTemplatePublished @event)
    {
        Status = TemplateStatus.Published;
        PublishedBy = @event.PublishedBy;
        LastPublishedDate = @event.LastPublishedDate;
        LastModifiedDate = @event.LastPublishedDate;

        if (PublishedDate == null)
            PublishedDate = @event.PublishedDate;
    }

    public void Apply(QuestionnaireTemplateUnpublishedToDraft @event)
    {
        Status = TemplateStatus.Draft;
        PublishedBy = string.Empty;
        LastModifiedDate = @event.ModifiedDate;
    }

    public void Apply(QuestionnaireTemplateArchived @event)
    {
        Status = TemplateStatus.Archived;
        LastModifiedDate = @event.ArchivedDate;
    }

    public void Apply(QuestionnaireTemplateRestoredFromArchive @event)
    {
        Status = TemplateStatus.Draft;
        LastModifiedDate = @event.RestoredDate;
    }
}