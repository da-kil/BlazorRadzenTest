using ti8m.BeachBreak.Core.Infrastructure.ValueObjects;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class QuestionItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public MultilingualText Title { get; set; } = new();
    public MultilingualText Description { get; set; } = new();
    public QuestionType Type { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public MultilingualOptions Options { get; set; } = new(); // For choice-based questions
}