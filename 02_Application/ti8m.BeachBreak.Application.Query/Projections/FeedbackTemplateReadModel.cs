using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.FeedbackTemplateAggregate.Events;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Query.Projections;

/// <summary>
/// Read model for feedback templates.
/// Projects domain events from FeedbackTemplate aggregate into a queryable model.
/// </summary>
public class FeedbackTemplateReadModel
{
    public Guid Id { get; set; }
    public string NameGerman { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public string DescriptionGerman { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;

    public List<EvaluationItem> Criteria { get; set; } = new();
    public List<TextSectionDefinition> TextSections { get; set; } = new();

    public int RatingScale { get; set; } = 10;
    public string ScaleLowLabel { get; set; } = "Poor";
    public string ScaleHighLabel { get; set; } = "Excellent";

    // Store as int array for query efficiency (can filter with Contains)
    public List<int> AllowedSourceTypes { get; set; } = new();

    // Ownership tracking
    public Guid CreatedByEmployeeId { get; set; }
    public ApplicationRole CreatedByRole { get; set; }

    // Lifecycle
    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;
    public DateTime CreatedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public Guid? PublishedByEmployeeId { get; set; }
    public bool IsDeleted { get; set; }

    // Helper properties
    public bool CanBeUsedForFeedback => Status == TemplateStatus.Published && !IsDeleted;
    public bool CanBeEdited => Status == TemplateStatus.Draft && !IsDeleted;
    public bool IsAvailable => !IsDeleted;

    public void Apply(FeedbackTemplateCreated @event)
    {
        Id = @event.AggregateId;
        NameGerman = @event.Name.German;
        NameEnglish = @event.Name.English;
        DescriptionGerman = @event.Description.German;
        DescriptionEnglish = @event.Description.English;
        Criteria = @event.Criteria;
        TextSections = @event.TextSections;
        RatingScale = @event.RatingScale;
        ScaleLowLabel = @event.ScaleLowLabel;
        ScaleHighLabel = @event.ScaleHighLabel;
        AllowedSourceTypes = @event.AllowedSourceTypes.Select(st => (int)st).ToList();
        CreatedByEmployeeId = @event.CreatedByEmployeeId;
        CreatedByRole = @event.CreatedByRole;
        CreatedDate = @event.CreatedDate;
        Status = TemplateStatus.Draft;
        IsDeleted = false;
    }

    public void Apply(FeedbackTemplateNameChanged @event)
    {
        NameGerman = @event.Name.German;
        NameEnglish = @event.Name.English;
    }

    public void Apply(FeedbackTemplateDescriptionChanged @event)
    {
        DescriptionGerman = @event.Description.German;
        DescriptionEnglish = @event.Description.English;
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
        AllowedSourceTypes = @event.AllowedSourceTypes.Select(st => (int)st).ToList();
    }

    public void Apply(FeedbackTemplatePublished @event)
    {
        Status = TemplateStatus.Published;
        PublishedByEmployeeId = @event.PublishedByEmployeeId;
        PublishedDate = @event.PublishedDate;
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
        // Cloning creates a NEW aggregate with a NEW ID
        // This event is for the NEW clone, not the source
        // So we initialize everything from the event
        Id = @event.NewTemplateId;
        NameGerman = @event.Name.German;
        NameEnglish = @event.Name.English;
        DescriptionGerman = @event.Description.German;
        DescriptionEnglish = @event.Description.English;
        Criteria = @event.Criteria;
        TextSections = @event.TextSections;
        RatingScale = @event.RatingScale;
        ScaleLowLabel = @event.ScaleLowLabel;
        ScaleHighLabel = @event.ScaleHighLabel;
        AllowedSourceTypes = @event.AllowedSourceTypes.Select(st => (int)st).ToList();
        CreatedByEmployeeId = @event.ClonedByEmployeeId;
        CreatedByRole = @event.ClonedByRole;
        CreatedDate = @event.CreatedDate;
        Status = TemplateStatus.Draft;
        IsDeleted = false;
    }
}
