using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;
using ti8m.BeachBreak.Domain.FeedbackTemplateAggregate.Events;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.FeedbackTemplateAggregate;

public class FeedbackTemplate : AggregateRoot
{
    public Translation Name { get; private set; } = new("", "");
    public Translation Description { get; private set; } = new("", "");

    public List<EvaluationItem> Criteria { get; private set; } = new();
    public List<TextSectionDefinition> TextSections { get; private set; } = new();
    public int RatingScale { get; private set; } = 10;
    public string ScaleLowLabel { get; private set; } = "Poor";
    public string ScaleHighLabel { get; private set; } = "Excellent";
    public List<FeedbackSourceType> AllowedSourceTypes { get; private set; } = new();

    // Ownership tracking (for authorization)
    public Guid CreatedByEmployeeId { get; private set; }
    public ApplicationRole CreatedByRole { get; private set; }

    public TemplateStatus Status { get; private set; } = TemplateStatus.Draft;
    public DateTime CreatedDate { get; private set; }
    public DateTime? PublishedDate { get; private set; }
    public Guid? PublishedByEmployeeId { get; private set; }
    public bool IsDeleted { get; private set; }

    private FeedbackTemplate() { }

    public FeedbackTemplate(
        Guid id,
        Translation name,
        Translation description,
        List<EvaluationItem> criteria,
        List<TextSectionDefinition> textSections,
        int ratingScale,
        string scaleLowLabel,
        string scaleHighLabel,
        List<FeedbackSourceType> allowedSourceTypes,
        Guid createdByEmployeeId,
        ApplicationRole createdByRole)
    {
        // Validation
        if (name == null || string.IsNullOrWhiteSpace(name.English))
            throw new ArgumentException("Name is required in English", nameof(name));

        if (criteria == null || criteria.Count == 0)
            throw new ArgumentException("At least one criterion is required", nameof(criteria));

        if (ratingScale < 2 || ratingScale > 10)
            throw new ArgumentException("Rating scale must be between 2 and 10", nameof(ratingScale));

        if (string.IsNullOrWhiteSpace(scaleLowLabel))
            throw new ArgumentException("Scale low label is required", nameof(scaleLowLabel));

        if (string.IsNullOrWhiteSpace(scaleHighLabel))
            throw new ArgumentException("Scale high label is required", nameof(scaleHighLabel));

        if (allowedSourceTypes == null || allowedSourceTypes.Count == 0)
            throw new ArgumentException("At least one allowed source type is required", nameof(allowedSourceTypes));

        if (createdByEmployeeId == Guid.Empty)
            throw new ArgumentException("Creator employee ID is required", nameof(createdByEmployeeId));

        // Raise domain event
        RaiseEvent(new FeedbackTemplateCreated(
            id,
            name,
            description ?? new Translation("", ""),
            criteria,
            textSections ?? new List<TextSectionDefinition>(),
            ratingScale,
            scaleLowLabel,
            scaleHighLabel,
            allowedSourceTypes,
            createdByEmployeeId,
            createdByRole,
            DateTime.UtcNow));
    }

    public bool CanBeEdited() => Status == TemplateStatus.Draft;

    public bool CanBeUsedForFeedback() => Status == TemplateStatus.Published;

    public void ChangeName(Translation name, Guid requestingEmployeeId, ApplicationRole requestingUserRole)
    {
        ValidateOwnership(requestingEmployeeId, requestingUserRole);

        if (name == null || string.IsNullOrWhiteSpace(name.English))
            throw new ArgumentException("Name is required in English", nameof(name));

        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        if (!Name.Equals(name))
        {
            RaiseEvent(new FeedbackTemplateNameChanged(name));
        }
    }

    public void ChangeDescription(Translation description, Guid requestingEmployeeId, ApplicationRole requestingUserRole)
    {
        ValidateOwnership(requestingEmployeeId, requestingUserRole);

        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        var newDescription = description ?? new Translation("", "");
        if (!Description.Equals(newDescription))
        {
            RaiseEvent(new FeedbackTemplateDescriptionChanged(newDescription));
        }
    }

    public void UpdateCriteria(List<EvaluationItem> criteria, Guid requestingEmployeeId, ApplicationRole requestingUserRole)
    {
        ValidateOwnership(requestingEmployeeId, requestingUserRole);

        if (criteria == null || criteria.Count == 0)
            throw new ArgumentException("At least one criterion is required", nameof(criteria));

        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        RaiseEvent(new FeedbackTemplateCriteriaChanged(criteria));
    }

    public void UpdateTextSections(List<TextSectionDefinition> textSections, Guid requestingEmployeeId, ApplicationRole requestingUserRole)
    {
        ValidateOwnership(requestingEmployeeId, requestingUserRole);

        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        RaiseEvent(new FeedbackTemplateTextSectionsChanged(textSections ?? new List<TextSectionDefinition>()));
    }

    public void UpdateRatingScale(int ratingScale, string scaleLowLabel, string scaleHighLabel, Guid requestingEmployeeId, ApplicationRole requestingUserRole)
    {
        ValidateOwnership(requestingEmployeeId, requestingUserRole);

        if (ratingScale < 2 || ratingScale > 10)
            throw new ArgumentException("Rating scale must be between 2 and 10", nameof(ratingScale));

        if (string.IsNullOrWhiteSpace(scaleLowLabel))
            throw new ArgumentException("Scale low label is required", nameof(scaleLowLabel));

        if (string.IsNullOrWhiteSpace(scaleHighLabel))
            throw new ArgumentException("Scale high label is required", nameof(scaleHighLabel));

        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        if (RatingScale != ratingScale || ScaleLowLabel != scaleLowLabel || ScaleHighLabel != scaleHighLabel)
        {
            RaiseEvent(new FeedbackTemplateRatingScaleChanged(ratingScale, scaleLowLabel, scaleHighLabel));
        }
    }

    public void UpdateSourceTypes(List<FeedbackSourceType> allowedSourceTypes, Guid requestingEmployeeId, ApplicationRole requestingUserRole)
    {
        ValidateOwnership(requestingEmployeeId, requestingUserRole);

        if (allowedSourceTypes == null || allowedSourceTypes.Count == 0)
            throw new ArgumentException("At least one allowed source type is required", nameof(allowedSourceTypes));

        if (!CanBeEdited())
            throw new InvalidOperationException("Template cannot be edited in current status");

        RaiseEvent(new FeedbackTemplateSourceTypesChanged(allowedSourceTypes));
    }

    public void Publish(Guid publishedByEmployeeId, ApplicationRole publishingUserRole)
    {
        ValidateOwnership(publishedByEmployeeId, publishingUserRole);

        if (publishedByEmployeeId == Guid.Empty)
            throw new ArgumentException("Publisher employee ID is required", nameof(publishedByEmployeeId));

        if (Status == TemplateStatus.Published)
            throw new InvalidOperationException("Template is already published");

        if (Status == TemplateStatus.Archived)
            throw new InvalidOperationException("Cannot publish an archived template");

        RaiseEvent(new FeedbackTemplatePublished(publishedByEmployeeId, DateTime.UtcNow));
    }

    public void Archive(Guid requestingEmployeeId, ApplicationRole requestingUserRole)
    {
        ValidateOwnership(requestingEmployeeId, requestingUserRole);

        if (Status == TemplateStatus.Archived)
            throw new InvalidOperationException("Template is already archived");

        RaiseEvent(new FeedbackTemplateArchived());
    }

    public void Delete(Guid requestingEmployeeId, ApplicationRole requestingUserRole)
    {
        ValidateOwnership(requestingEmployeeId, requestingUserRole);

        if (IsDeleted)
            throw new InvalidOperationException("Template is already deleted");

        RaiseEvent(new FeedbackTemplateDeleted());
    }

    /// <summary>
    /// Validates that the requesting user has permission to modify this template.
    /// - Admin can edit any template
    /// - HR (HR, HRLead) can edit HR templates
    /// - TeamLead can only edit their own templates
    /// </summary>
    private void ValidateOwnership(Guid requestingEmployeeId, ApplicationRole requestingUserRole)
    {
        // Admin can edit any template
        if (requestingUserRole == ApplicationRole.Admin)
            return;

        // HR users can edit HR templates
        if (CreatedByRole >= ApplicationRole.HR && requestingUserRole >= ApplicationRole.HR)
            return;

        // TeamLead can only edit their own templates
        if (CreatedByRole == ApplicationRole.TeamLead && CreatedByEmployeeId == requestingEmployeeId)
            return;

        throw new UnauthorizedAccessException(
            "You do not have permission to modify this template. " +
            (CreatedByRole == ApplicationRole.TeamLead
                ? "TeamLead templates can only be edited by their creator."
                : "HR templates can only be edited by HR users."));
    }

    /// <summary>
    /// Creates a clone of an existing feedback template with new IDs.
    /// The cloned template is always in Draft status with reset timestamps.
    /// </summary>
    public static FeedbackTemplate CloneFrom(
        Guid newTemplateId,
        FeedbackTemplate source,
        Guid clonedByEmployeeId,
        ApplicationRole clonedByRole,
        string namePrefix = "Copy of ")
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (source.IsDeleted)
            throw new InvalidOperationException("Cannot clone a deleted template");

        if (clonedByEmployeeId == Guid.Empty)
            throw new ArgumentException("Cloning employee ID is required", nameof(clonedByEmployeeId));

        // Generate new name - add prefix to both languages
        var clonedName = new Translation(
            $"{namePrefix}{source.Name.German}",
            $"{namePrefix}{source.Name.English}");

        // Create new aggregate instance
        var clonedTemplate = new FeedbackTemplate();
        clonedTemplate.RaiseEvent(new FeedbackTemplateCloned(
            newTemplateId,
            source.Id,
            clonedName,
            source.Description,
            source.Criteria,
            source.TextSections,
            source.RatingScale,
            source.ScaleLowLabel,
            source.ScaleHighLabel,
            source.AllowedSourceTypes,
            clonedByEmployeeId,
            clonedByRole,
            DateTime.UtcNow
        ));

        return clonedTemplate;
    }

    // Apply methods for event sourcing
    public void Apply(FeedbackTemplateCreated @event)
    {
        Id = @event.AggregateId;
        Name = @event.Name;
        Description = @event.Description;
        Criteria = @event.Criteria;
        TextSections = @event.TextSections;
        RatingScale = @event.RatingScale;
        ScaleLowLabel = @event.ScaleLowLabel;
        ScaleHighLabel = @event.ScaleHighLabel;
        AllowedSourceTypes = @event.AllowedSourceTypes;
        CreatedByEmployeeId = @event.CreatedByEmployeeId;
        CreatedByRole = @event.CreatedByRole;
        Status = TemplateStatus.Draft;
        CreatedDate = @event.CreatedDate;
        IsDeleted = false;
    }

    public void Apply(FeedbackTemplateNameChanged @event)
    {
        Name = @event.Name;
    }

    public void Apply(FeedbackTemplateDescriptionChanged @event)
    {
        Description = @event.Description;
    }

    public void Apply(FeedbackTemplateCriteriaChanged @event)
    {
        Criteria = @event.Criteria;
    }

    public void Apply(FeedbackTemplateTextSectionsChanged @event)
    {
        TextSections = @event.TextSections;
    }

    public void Apply(FeedbackTemplateRatingScaleChanged @event)
    {
        RatingScale = @event.RatingScale;
        ScaleLowLabel = @event.ScaleLowLabel;
        ScaleHighLabel = @event.ScaleHighLabel;
    }

    public void Apply(FeedbackTemplateSourceTypesChanged @event)
    {
        AllowedSourceTypes = @event.AllowedSourceTypes;
    }

    public void Apply(FeedbackTemplatePublished @event)
    {
        Status = TemplateStatus.Published;
        PublishedDate = @event.PublishedDate;
        PublishedByEmployeeId = @event.PublishedByEmployeeId;
    }

    public void Apply(FeedbackTemplateArchived @event)
    {
        Status = TemplateStatus.Archived;
    }

    public void Apply(FeedbackTemplateDeleted @event)
    {
        IsDeleted = true;
    }

    public void Apply(FeedbackTemplateCloned @event)
    {
        Id = @event.NewTemplateId;
        Name = @event.Name;
        Description = @event.Description;
        Criteria = @event.Criteria;
        TextSections = @event.TextSections;
        RatingScale = @event.RatingScale;
        ScaleLowLabel = @event.ScaleLowLabel;
        ScaleHighLabel = @event.ScaleHighLabel;
        AllowedSourceTypes = @event.AllowedSourceTypes;
        CreatedByEmployeeId = @event.ClonedByEmployeeId;
        CreatedByRole = @event.ClonedByRole;
        Status = TemplateStatus.Draft;  // Always draft
        CreatedDate = @event.CreatedDate;
        PublishedDate = null;  // Reset publication data
        PublishedByEmployeeId = null;
        IsDeleted = false;
    }
}
