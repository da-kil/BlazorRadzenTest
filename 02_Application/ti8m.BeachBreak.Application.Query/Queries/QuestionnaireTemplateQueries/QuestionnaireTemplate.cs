using ti8m.BeachBreak.Core.Infrastructure.ValueObjects;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;

public class QuestionnaireTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public MultilingualText Name { get; set; } = new();
    public MultilingualText Description { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? LastModified { get; set; }

    // Semantic status properties
    public bool IsActive { get; set; } = true;           // System availability
    public bool IsPublished { get; set; } = false;      // Ready for assignments
    public DateTime? PublishedDate { get; set; }        // First publish timestamp
    public DateTime? LastPublishedDate { get; set; }    // Most recent publish
    public string PublishedBy { get; set; } = string.Empty; // Who published it

    public List<QuestionSection> Sections { get; set; } = new();
    public QuestionnaireSettings Settings { get; set; } = new();

    // Business logic properties
    public bool CanBeAssigned => IsActive && IsPublished;
    public bool IsAvailableForEditing => IsActive;
    public bool IsVisibleInCatalog => IsActive && IsPublished;

    // Status determination
    public TemplateStatus Status => (IsActive, IsPublished) switch
    {
        (true, true)   => TemplateStatus.Published,
        (true, false)  => TemplateStatus.Draft,
        (false, true)  => TemplateStatus.PublishedInactive,
        (false, false) => TemplateStatus.Inactive,
    };
}

public enum TemplateStatus
{
    Draft,              // Active but not published
    Published,          // Active and published
    PublishedInactive,  // Published but temporarily disabled
    Inactive            // Completely disabled
}
