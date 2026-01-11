using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

namespace ti8m.BeachBreak.Application.Query.Projections;

public class QuestionnaireTemplateReadModel
{
    public Guid Id { get; set; }
    public string NameGerman { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public string DescriptionGerman { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public bool RequiresManagerReview { get; set; } = true;
    public bool IsCustomizable { get; set; } = false;
    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;
    public DateTime? PublishedDate { get; set; }
    public DateTime? LastPublishedDate { get; set; }
    public Guid? PublishedByEmployeeId { get; set; }
    public List<QuestionSection> Sections { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public bool IsDeleted { get; set; }

    public bool CanBeAssigned => Status == TemplateStatus.Published;
    public bool IsAvailableForEditing => Status == TemplateStatus.Draft;
    public bool IsVisibleInCatalog => Status == TemplateStatus.Published && !IsDeleted;

    public void Apply(QuestionnaireTemplateCreated @event)
    {
        Id = @event.AggregateId;
        NameGerman = @event.Name.German;
        NameEnglish = @event.Name.English;
        DescriptionGerman = @event.Description.German;
        DescriptionEnglish = @event.Description.English;
        CategoryId = @event.CategoryId;
        RequiresManagerReview = @event.RequiresManagerReview;
        IsCustomizable = @event.IsCustomizable;
        Sections = MapDomainSectionsToQuerySections(@event.Sections);
        Status = TemplateStatus.Draft;
        CreatedDate = @event.CreatedDate;
        IsDeleted = false;
    }

    public void Apply(QuestionnaireTemplateNameChanged @event)
    {
        NameGerman = @event.Name.German;
        NameEnglish = @event.Name.English;
    }

    public void Apply(QuestionnaireTemplateDescriptionChanged @event)
    {
        DescriptionGerman = @event.Description.German;
        DescriptionEnglish = @event.Description.English;
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

    public void Apply(QuestionnaireTemplateSectionsChanged @event)
    {
        Sections = MapDomainSectionsToQuerySections(@event.Sections);
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
        NameGerman = @event.Name.German;
        NameEnglish = @event.Name.English;
        DescriptionGerman = @event.Description.German;
        DescriptionEnglish = @event.Description.English;
        CategoryId = @event.CategoryId;
        RequiresManagerReview = @event.RequiresManagerReview;
        IsCustomizable = @event.IsCustomizable;
        Sections = MapDomainSectionsToQuerySections(@event.Sections);
        Status = TemplateStatus.Draft;
        CreatedDate = @event.CreatedDate;
        PublishedDate = null;
        LastPublishedDate = null;
        PublishedByEmployeeId = null;
        IsDeleted = false;
    }

    private static List<QuestionSection> MapDomainSectionsToQuerySections(List<Domain.QuestionnaireTemplateAggregate.Events.QuestionSectionData> data)
    {
        return data.Select(s => new QuestionSection
        {
            Id = s.Id,
            TitleGerman = s.Title.German,
            TitleEnglish = s.Title.English,
            DescriptionGerman = s.Description.German,
            DescriptionEnglish = s.Description.English,
            Order = s.Order,
            CompletionRole = s.CompletionRole.ToString(),
            Type = s.Type.ToString(),
            Configuration = s.Configuration
        }).ToList();
    }
}

