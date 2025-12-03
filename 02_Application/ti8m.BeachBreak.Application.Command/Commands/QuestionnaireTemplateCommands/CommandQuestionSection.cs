using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class CommandQuestionSection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TitleGerman { get; set; } = string.Empty;
    public string TitleEnglish { get; set; } = string.Empty;
    public string DescriptionGerman { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public CompletionRole CompletionRole { get; set; } = CompletionRole.Employee;
    public List<CommandQuestionItem> Questions { get; set; } = new();
}