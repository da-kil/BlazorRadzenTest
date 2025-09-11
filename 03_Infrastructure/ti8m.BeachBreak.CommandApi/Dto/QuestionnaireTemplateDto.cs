namespace ti8m.BeachBreak.CommandApi.Dto;

public class QuestionnaireTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<QuestionSectionDto> Sections { get; set; } = new();
    public QuestionnaireSettingsDto Settings { get; set; } = new();
}
