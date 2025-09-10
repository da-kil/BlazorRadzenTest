namespace ti8m.BeachBreak.QueryApi.Dto;

public class QuestionnaireTemplateDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? LastModified { get; set; }
    public bool IsActive { get; set; } = true;
    public List<QuestionSectionDto> Sections { get; set; } = new();
    public QuestionnaireSettingsDto Settings { get; set; } = new();
}
