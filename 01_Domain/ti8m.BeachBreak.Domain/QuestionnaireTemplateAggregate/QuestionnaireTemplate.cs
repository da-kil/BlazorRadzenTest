using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Services;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

public class QuestionnaireTemplate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public bool RequiresManagerReview { get; private set; } = true;

    public TemplateStatus Status { get; private set; } = TemplateStatus.Draft;
    public DateTime? PublishedDate { get; private set; }
    public DateTime? LastPublishedDate { get; private set; }
    public string PublishedBy { get; private set; } = string.Empty;

    public List<QuestionSection> Sections { get; private set; } = new();
    public QuestionnaireSettings Settings { get; private set; } = new();

    public DateTime CreatedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    private QuestionnaireTemplate() { }

    public QuestionnaireTemplate(
        Guid id,
        string name,
        string description,
        Guid categoryId,
        bool requiresManagerReview = true,
        List<QuestionSection>? sections = null,
        QuestionnaireSettings? settings = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        RaiseEvent(new QuestionnaireTemplateCreated(
            id,
            name,
            description ?? string.Empty,
            categoryId,
            requiresManagerReview,
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
            RaiseEvent(new QuestionnaireTemplateNameChanged(Id, name));
        }
    }

    public void ChangeDescription(string description)
    {
        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        var newDescription = description ?? string.Empty;
        if (!string.Equals(Description, newDescription, StringComparison.Ordinal))
        {
            RaiseEvent(new QuestionnaireTemplateDescriptionChanged(Id, newDescription));
        }
    }

    public void ChangeCategory(Guid categoryId)
    {
        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        if (CategoryId != categoryId)
        {
            RaiseEvent(new QuestionnaireTemplateCategoryChanged(Id, categoryId));
        }
    }

    public void UpdateSections(List<QuestionSection> sections)
    {
        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        RaiseEvent(new QuestionnaireTemplateSectionsChanged(Id, sections ?? new()));
    }

    public void UpdateSettings(QuestionnaireSettings settings)
    {
        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        RaiseEvent(new QuestionnaireTemplateSettingsChanged(Id, settings ?? new()));
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

    public async Task UnpublishToDraftAsync(IQuestionnaireAssignmentService assignmentService, CancellationToken cancellationToken = default)
    {
        if (await assignmentService.HasActiveAssignmentsAsync(Id, cancellationToken))
        {
            var assignmentCount = await assignmentService.GetActiveAssignmentCountAsync(Id, cancellationToken);
            throw new InvalidOperationException(
                $"Cannot unpublish questionnaire template to draft: {assignmentCount} active assignment(s) exist.");
        }

        if (Status != TemplateStatus.Published)
            throw new InvalidOperationException("Only published templates can be unpublished to draft");

        RaiseEvent(new QuestionnaireTemplateUnpublishedToDraft(Id));
    }

    public void Archive()
    {
        if (Status == TemplateStatus.Archived)
            throw new InvalidOperationException("Template is already archived");

        RaiseEvent(new QuestionnaireTemplateArchived(Id));
    }

    public void RestoreFromArchive()
    {
        if (Status != TemplateStatus.Archived)
            throw new InvalidOperationException("Only archived templates can be restored");

        RaiseEvent(new QuestionnaireTemplateRestoredFromArchive(Id));
    }

    public async Task DeleteAsync(IQuestionnaireAssignmentService assignmentService, CancellationToken cancellationToken = default)
    {
        if (await assignmentService.HasActiveAssignmentsAsync(Id, cancellationToken))
        {
            var assignmentCount = await assignmentService.GetActiveAssignmentCountAsync(Id, cancellationToken);
            throw new InvalidOperationException(
                $"Cannot delete questionnaire template: {assignmentCount} active assignment(s) exist. " +
                "Complete or cancel these assignments first, or archive the template instead.");
        }

        RaiseEvent(new QuestionnaireTemplateDeleted(Id));
    }

    public bool CanBeDeleted()
    {
        // Template can only be deleted if it's not archived (archived templates should stay for audit purposes)
        return Status != TemplateStatus.Archived;
    }

    /// <summary>
    /// Validates that section completion roles match the review requirement.
    /// When RequiresManagerReview is false, all sections must be Employee-only.
    /// </summary>
    public void ValidateSectionCompletionRoles()
    {
        if (!RequiresManagerReview)
        {
            var nonEmployeeSections = Sections
                .Where(s => s.CompletionRole != CompletionRole.Employee)
                .ToList();

            if (nonEmployeeSections.Any())
            {
                var sectionTitles = string.Join(", ", nonEmployeeSections.Select(s =>
                    string.IsNullOrWhiteSpace(s.Title) ? $"Section {s.Order + 1}" : s.Title));

                throw new InvalidOperationException(
                    $"When manager review is not required, all sections must be completed by Employee only. " +
                    $"Found {nonEmployeeSections.Count} section(s) with Manager or Both completion roles: {sectionTitles}");
            }
        }
    }

    /// <summary>
    /// Creates a clone of an existing questionnaire template with new IDs.
    /// The cloned template is always in Draft status with reset timestamps.
    /// </summary>
    public static QuestionnaireTemplate CloneFrom(
        Guid newTemplateId,
        QuestionnaireTemplate source,
        string namePrefix = "Copy of ")
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (source.IsDeleted)
            throw new InvalidOperationException("Cannot clone a deleted template");

        // Generate new name
        var clonedName = $"{namePrefix}{source.Name}";

        // Deep copy sections with new IDs
        var clonedSections = source.Sections.Select(section =>
        {
            var clonedQuestions = section.Questions.Select(question =>
                new QuestionItem(
                    Guid.NewGuid(),  // New question ID
                    question.Title,
                    question.Description,
                    question.Type,
                    question.Order,
                    question.IsRequired,
                    new Dictionary<string, object>(question.Configuration),  // Deep copy
                    new List<string>(question.Options)  // Deep copy
                )
            ).ToList();

            return new QuestionSection(
                Guid.NewGuid(),  // New section ID
                section.Title,
                section.Description,
                section.Order,
                section.IsRequired,
                section.CompletionRole,
                clonedQuestions
            );
        }).ToList();

        // Clone settings (value object, already immutable)
        var clonedSettings = new QuestionnaireSettings(
            source.Settings.AllowSaveProgress,
            source.Settings.ShowProgressBar,
            source.Settings.RequireAllSections,
            source.Settings.SuccessMessage,
            source.Settings.IncompleteMessage,
            source.Settings.TimeLimit,
            source.Settings.AllowReviewBeforeSubmit
        );

        // Create new aggregate instance
        var clonedTemplate = new QuestionnaireTemplate();
        clonedTemplate.RaiseEvent(new QuestionnaireTemplateCloned(
            newTemplateId,
            source.Id,
            clonedName,
            source.Description,
            source.CategoryId,
            source.RequiresManagerReview,
            clonedSections,
            clonedSettings,
            DateTime.UtcNow
        ));

        return clonedTemplate;
    }

    // Apply methods for event sourcing
    public void Apply(QuestionnaireTemplateCreated @event)
    {
        Id = @event.AggregateId;
        Name = @event.Name;
        Description = @event.Description;
        CategoryId = @event.CategoryId;
        RequiresManagerReview = @event.RequiresManagerReview;
        Sections = @event.Sections;
        Settings = @event.Settings;
        Status = TemplateStatus.Draft;
        CreatedDate = @event.CreatedDate;
        IsDeleted = false;
    }

    public void Apply(QuestionnaireTemplateNameChanged @event)
    {
        Name = @event.Name;
    }

    public void Apply(QuestionnaireTemplateDescriptionChanged @event)
    {
        Description = @event.Description;
    }

    public void Apply(QuestionnaireTemplateCategoryChanged @event)
    {
        CategoryId = @event.CategoryId;
    }

    public void Apply(QuestionnaireTemplateSectionsChanged @event)
    {
        Sections = @event.Sections;
    }

    public void Apply(QuestionnaireTemplateSettingsChanged @event)
    {
        Settings = @event.Settings;
    }

    public void Apply(QuestionnaireTemplatePublished @event)
    {
        Status = TemplateStatus.Published;
        PublishedBy = @event.PublishedBy;
        LastPublishedDate = @event.LastPublishedDate;

        if (PublishedDate == null)
            PublishedDate = @event.PublishedDate;
    }

    public void Apply(QuestionnaireTemplateUnpublishedToDraft @event)
    {
        Status = TemplateStatus.Draft;
        PublishedBy = string.Empty;
    }

    public void Apply(QuestionnaireTemplateArchived @event)
    {
        Status = TemplateStatus.Archived;
    }

    public void Apply(QuestionnaireTemplateRestoredFromArchive @event)
    {
        Status = TemplateStatus.Draft;
    }

    public void Apply(QuestionnaireTemplateDeleted @event)
    {
        IsDeleted = true;
    }

    public void Apply(QuestionnaireTemplateCloned @event)
    {
        Id = @event.NewTemplateId;
        Name = @event.Name;
        Description = @event.Description;
        CategoryId = @event.CategoryId;
        RequiresManagerReview = @event.RequiresManagerReview;
        Sections = @event.Sections;
        Settings = @event.Settings;
        Status = TemplateStatus.Draft;  // Always draft
        CreatedDate = @event.CreatedDate;
        PublishedDate = null;  // Reset publication data
        LastPublishedDate = null;
        PublishedBy = string.Empty;
        IsDeleted = false;
    }
}