namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class CommandQuestionItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public List<string> Options { get; set; } = new();
}