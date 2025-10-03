using ti8m.BeachBreak.Domain.QuestionnaireAggregate.Events;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;

namespace ti8m.BeachBreak.Application.Query.Projections;

public class QuestionnaireTemplateReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;
    public DateTime? PublishedDate { get; set; }
    public DateTime? LastPublishedDate { get; set; }
    public string PublishedBy { get; set; } = string.Empty;
    public List<QuestionSection> Sections { get; set; } = new();
    public QuestionnaireSettings Settings { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public bool IsDeleted { get; set; }

    public bool CanBeAssigned => Status == TemplateStatus.Published;
    public bool IsAvailableForEditing => Status == TemplateStatus.Draft;
    public bool IsVisibleInCatalog => Status == TemplateStatus.Published && !IsDeleted;

    public void Apply(QuestionnaireTemplateCreated @event)
    {
        Id = @event.AggregateId;
        Name = @event.Name;
        Description = @event.Description;
        CategoryId = @event.CategoryId;
        Sections = MapDomainSectionsToQuerySections(@event.Sections);
        Settings = MapDomainSettingsToQuerySettings(@event.Settings);
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
        Sections = MapDomainSectionsToQuerySections(@event.Sections);
    }

    public void Apply(QuestionnaireTemplateSettingsChanged @event)
    {
        Settings = MapDomainSettingsToQuerySettings(@event.Settings);
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

    private static List<QuestionSection> MapDomainSectionsToQuerySections(List<Domain.QuestionnaireAggregate.QuestionSection> domainSections)
    {
        return domainSections.Select(ds => new QuestionSection
        {
            Id = ds.Id,
            Title = ds.Title,
            Description = ds.Description,
            Order = ds.Order,
            IsRequired = ds.IsRequired,
            CompletionRole = ds.CompletionRole.ToString(),
            Questions = ds.Questions.Select(dq => new QuestionItem
            {
                Id = dq.Id,
                Title = dq.Title,
                Description = dq.Description,
                Type = (QuestionType)(int)dq.Type,
                IsRequired = dq.IsRequired,
                Order = dq.Order,
                Configuration = dq.Configuration,
                Options = dq.Options
            }).ToList()
        }).ToList();
    }

    private static QuestionnaireSettings MapDomainSettingsToQuerySettings(Domain.QuestionnaireAggregate.QuestionnaireSettings domainSettings)
    {
        return new QuestionnaireSettings
        {
            AllowSaveProgress = domainSettings.AllowSaveProgress,
            ShowProgressBar = domainSettings.ShowProgressBar,
            RequireAllSections = domainSettings.RequireAllSections,
            SuccessMessage = domainSettings.SuccessMessage,
            IncompleteMessage = domainSettings.IncompleteMessage,
            TimeLimit = domainSettings.TimeLimit,
            AllowReviewBeforeSubmit = domainSettings.AllowReviewBeforeSubmit
        };
    }
}

