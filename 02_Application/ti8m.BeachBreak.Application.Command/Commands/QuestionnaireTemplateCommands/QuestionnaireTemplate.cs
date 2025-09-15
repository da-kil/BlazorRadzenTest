namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class QuestionnaireTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<QuestionSection> Sections { get; set; } = new();
    public QuestionnaireSettings Settings { get; set; } = new();
}
