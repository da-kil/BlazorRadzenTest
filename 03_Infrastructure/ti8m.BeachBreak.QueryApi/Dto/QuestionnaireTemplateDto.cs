namespace ti8m.BeachBreak.QueryApi.Dto;

public class QuestionnaireTemplateDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? LastModified { get; set; }

    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;
    public DateTime? PublishedDate { get; set; }        // First publish timestamp
    public DateTime? LastPublishedDate { get; set; }    // Most recent publish
    public string PublishedBy { get; set; } = string.Empty; // Who published it

    public List<QuestionSectionDto> Sections { get; set; } = new();
    public QuestionnaireSettingsDto Settings { get; set; } = new();

    // Business logic properties derived from status
    public bool CanBeAssigned => Status == TemplateStatus.Published;
    public bool IsAvailableForEditing => Status == TemplateStatus.Draft;
    public bool IsVisibleInCatalog => Status == TemplateStatus.Published;
}

public enum TemplateStatus
{
    Draft = 0,      // Template can be edited, not assignable
    Published = 1,  // Template is read-only, can be assigned
    Archived = 2    // Template is inactive, cannot be assigned or edited
}
