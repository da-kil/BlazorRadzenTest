namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class CommandQuestionnaireTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public List<CommandQuestionSection> Sections { get; set; } = new();
    public CommandQuestionnaireSettings Settings { get; set; } = new();
}