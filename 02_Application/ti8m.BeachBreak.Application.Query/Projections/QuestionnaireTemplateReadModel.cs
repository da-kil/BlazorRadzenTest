using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

namespace ti8m.BeachBreak.Application.Query.Projections;

public class QuestionnaireTemplateReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public bool RequiresManagerReview { get; set; } = true;
    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;
    public DateTime? PublishedDate { get; set; }
    public DateTime? LastPublishedDate { get; set; }
    public string PublishedBy { get; set; } = string.Empty;
    public List<QuestionSection> Sections { get; set; } = new();
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
        RequiresManagerReview = @event.RequiresManagerReview;
        Sections = MapDomainSectionsToQuerySections(@event.Sections);
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

    public void Apply(QuestionnaireTemplateSectionsChanged @event)
    {
        Sections = MapDomainSectionsToQuerySections(@event.Sections);
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
        Sections = MapDomainSectionsToQuerySections(@event.Sections);
        Status = TemplateStatus.Draft;
        CreatedDate = @event.CreatedDate;
        PublishedDate = null;
        LastPublishedDate = null;
        PublishedBy = string.Empty;
        IsDeleted = false;
    }

    private static List<QuestionSection> MapDomainSectionsToQuerySections(List<Domain.QuestionnaireTemplateAggregate.QuestionSection> domainSections)
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
}

