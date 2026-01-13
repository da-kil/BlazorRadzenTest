using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Services;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

public class QuestionnaireTemplate : AggregateRoot
{
    public Translation Name { get; private set; } = new("", "");
    public Translation Description { get; private set; } = new("", "");
    public Guid CategoryId { get; private set; }
    public bool RequiresManagerReview { get; private set; } = true;
    public bool IsCustomizable { get; private set; } = false;
    public bool AutoInitialize { get; private set; } = false;

    public TemplateStatus Status { get; private set; } = TemplateStatus.Draft;
    public DateTime? PublishedDate { get; private set; }
    public DateTime? LastPublishedDate { get; private set; }
    public Guid? PublishedByEmployeeId { get; private set; }

    public List<QuestionSection> Sections { get; private set; } = new();

    public DateTime CreatedDate { get; private set; }
    public bool IsDeleted { get; private set; }

    private QuestionnaireTemplate() { }

    public QuestionnaireTemplate(
        Guid id,
        Translation name,
        Translation description,
        Guid categoryId,
        bool requiresManagerReview = true,
        bool isCustomizable = false,
        bool autoInitialize = false,
        List<QuestionSection>? sections = null)
    {
        if (name == null || (string.IsNullOrWhiteSpace(name.English) && string.IsNullOrWhiteSpace(name.German)))
            throw new ArgumentException("Name is required in at least one language", nameof(name));

        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category is required", nameof(categoryId));

        RaiseEvent(new QuestionnaireTemplateCreated(
            id,
            name,
            description ?? new Translation("", ""),
            categoryId,
            requiresManagerReview,
            isCustomizable,
            autoInitialize,
            QuestionnaireTemplateEventDataMapper.MapSectionsToData(sections ?? new()),
            DateTime.UtcNow));
    }

    public bool CanBeAssignedToEmployee() => Status == TemplateStatus.Published;

    public bool CanBeEdited() => Status == TemplateStatus.Draft;

    public void ChangeName(Translation name)
    {
        if (name == null || (string.IsNullOrWhiteSpace(name.English) && string.IsNullOrWhiteSpace(name.German)))
            throw new ArgumentException("Name is required in at least one language", nameof(name));

        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        if (!Name.Equals(name))
        {
            RaiseEvent(new QuestionnaireTemplateNameChanged(name));
        }
    }

    public void ChangeDescription(Translation description)
    {
        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        var newDescription = description ?? new Translation("", "");
        if (!Description.Equals(newDescription))
        {
            RaiseEvent(new QuestionnaireTemplateDescriptionChanged(newDescription));
        }
    }

    public void ChangeCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category is required", nameof(categoryId));

        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        if (CategoryId != categoryId)
        {
            RaiseEvent(new QuestionnaireTemplateCategoryChanged(categoryId));
        }
    }

    public async Task ChangeReviewRequirementAsync(
        bool requiresManagerReview,
        IQuestionnaireAssignmentService assignmentService,
        CancellationToken cancellationToken = default)
    {
        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        // If RequiresManagerReview is being changed, check for existing assignments
        if (RequiresManagerReview != requiresManagerReview)
        {
            if (await assignmentService.HasActiveAssignmentsAsync(Id, cancellationToken))
            {
                var assignmentCount = await assignmentService.GetActiveAssignmentCountAsync(Id, cancellationToken);
                throw new InvalidOperationException(
                    $"Cannot change review requirement: {assignmentCount} active assignment(s) exist. " +
                    "Complete or withdraw these assignments first, or create a new template with the desired setting.");
            }

            RaiseEvent(new QuestionnaireTemplateReviewRequirementChanged(requiresManagerReview));
        }
    }

    public async Task ChangeCustomizabilityAsync(
        bool isCustomizable,
        IQuestionnaireAssignmentService assignmentService,
        CancellationToken cancellationToken = default)
    {
        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        // If IsCustomizable is being changed, check for existing assignments
        if (IsCustomizable != isCustomizable)
        {
            if (await assignmentService.HasActiveAssignmentsAsync(Id, cancellationToken))
            {
                var assignmentCount = await assignmentService.GetActiveAssignmentCountAsync(Id, cancellationToken);
                throw new InvalidOperationException(
                    $"Cannot change customizability setting: {assignmentCount} active assignment(s) exist. " +
                    "Complete or withdraw these assignments first, or create a new template with the desired setting.");
            }

            RaiseEvent(new QuestionnaireTemplateCustomizabilityChanged(isCustomizable));
        }
    }

    public async Task ChangeAutoInitializeAsync(
        bool autoInitialize,
        IQuestionnaireAssignmentService assignmentService,
        CancellationToken cancellationToken = default)
    {
        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        // If AutoInitialize is being changed, check for existing assignments
        if (AutoInitialize != autoInitialize)
        {
            if (await assignmentService.HasActiveAssignmentsAsync(Id, cancellationToken))
            {
                var assignmentCount = await assignmentService.GetActiveAssignmentCountAsync(Id, cancellationToken);
                throw new InvalidOperationException(
                    $"Cannot change auto-initialize setting: {assignmentCount} active assignment(s) exist. " +
                    "Complete or withdraw these assignments first, or create a new template with the desired setting.");
            }

            RaiseEvent(new QuestionnaireTemplateAutoInitializeChanged(autoInitialize));
        }
    }

    public void UpdateSections(List<QuestionSection> sections)
    {
        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        // Validate configurations match types before updating
        foreach (var section in sections ?? new())
        {
            section.ValidateConfigurationMatchesType();
        }

        RaiseEvent(new QuestionnaireTemplateSectionsChanged(QuestionnaireTemplateEventDataMapper.MapSectionsToData(sections ?? new())));
    }


    public void Publish(Guid publishedByEmployeeId)
    {
        if (publishedByEmployeeId == Guid.Empty)
            throw new ArgumentException("Publisher employee ID is required", nameof(publishedByEmployeeId));

        if (Status == TemplateStatus.Archived)
            throw new InvalidOperationException("Cannot publish an archived template");

        if (Status == TemplateStatus.Published)
            throw new InvalidOperationException("Template is already published");

        // Validate configurations match types before publishing
        ValidateSectionConfigurations();

        var now = DateTime.UtcNow;
        var publishedDate = PublishedDate ?? now;

        RaiseEvent(new QuestionnaireTemplatePublished(publishedByEmployeeId, publishedDate, now));
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

        RaiseEvent(new QuestionnaireTemplateUnpublishedToDraft());
    }

    public void Archive()
    {
        if (Status == TemplateStatus.Archived)
            throw new InvalidOperationException("Template is already archived");

        RaiseEvent(new QuestionnaireTemplateArchived());
    }

    public void RestoreFromArchive()
    {
        if (Status != TemplateStatus.Archived)
            throw new InvalidOperationException("Only archived templates can be restored");

        RaiseEvent(new QuestionnaireTemplateRestoredFromArchive());
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

        RaiseEvent(new QuestionnaireTemplateDeleted());
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
                    string.IsNullOrWhiteSpace(s.Title.English) ? $"Section {s.Order + 1}" : s.Title.English));

                throw new InvalidOperationException(
                    $"When manager review is not required, all sections must be completed by Employee only. " +
                    $"Found {nonEmployeeSections.Count} section(s) with Manager or Both completion roles: {sectionTitles}");
            }
        }
    }

    /// <summary>
    /// Validates that each section's configuration matches its type.
    /// Ensures data integrity between section type and configuration object.
    /// </summary>
    public void ValidateSectionConfigurations()
    {
        foreach (var section in Sections)
        {
            section.ValidateConfigurationMatchesType();
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

        // Generate new name - add prefix to both languages
        var clonedName = new Translation(
            $"{namePrefix}{source.Name.German}",
            $"{namePrefix}{source.Name.English}");

        // Deep copy sections with new IDs
        var clonedSections = source.Sections.Select(section =>
            new QuestionSection(
                Guid.NewGuid(),  // New section ID
                section.Title,
                section.Description,
                section.Order,
                section.CompletionRole,
                section.Type,
                section.Configuration  // Configuration objects are immutable, safe to share
            )
        ).ToList();

        // Create new aggregate instance
        var clonedTemplate = new QuestionnaireTemplate();
        clonedTemplate.RaiseEvent(new QuestionnaireTemplateCloned(
            newTemplateId,
            source.Id,
            clonedName,
            source.Description,
            source.CategoryId,
            source.RequiresManagerReview,
            source.IsCustomizable,
            source.AutoInitialize,
            QuestionnaireTemplateEventDataMapper.MapSectionsToData(clonedSections),
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
        IsCustomizable = @event.IsCustomizable;
        AutoInitialize = @event.AutoInitialize;
        Sections = QuestionnaireTemplateEventDataMapper.MapDataToSections(@event.Sections);
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

    public void Apply(QuestionnaireTemplateReviewRequirementChanged @event)
    {
        RequiresManagerReview = @event.RequiresManagerReview;
    }

    public void Apply(QuestionnaireTemplateCustomizabilityChanged @event)
    {
        IsCustomizable = @event.IsCustomizable;
    }

    public void Apply(QuestionnaireTemplateAutoInitializeChanged @event)
    {
        AutoInitialize = @event.AutoInitialize;
    }

    public void Apply(QuestionnaireTemplateSectionsChanged @event)
    {
        Sections = QuestionnaireTemplateEventDataMapper.MapDataToSections(@event.Sections);
    }

    public void Apply(QuestionnaireTemplatePublished @event)
    {
        Status = TemplateStatus.Published;
        PublishedByEmployeeId = @event.PublishedByEmployeeId;
        LastPublishedDate = @event.LastPublishedDate;

        if (PublishedDate == null)
            PublishedDate = @event.PublishedDate;
    }

    public void Apply(QuestionnaireTemplateUnpublishedToDraft @event)
    {
        Status = TemplateStatus.Draft;
        PublishedByEmployeeId = null;
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
        IsCustomizable = @event.IsCustomizable;
        AutoInitialize = @event.AutoInitialize;
        Sections = QuestionnaireTemplateEventDataMapper.MapDataToSections(@event.Sections);
        Status = TemplateStatus.Draft;  // Always draft
        CreatedDate = @event.CreatedDate;
        PublishedDate = null;  // Reset publication data
        LastPublishedDate = null;
        PublishedByEmployeeId = null;
        IsDeleted = false;
    }
}