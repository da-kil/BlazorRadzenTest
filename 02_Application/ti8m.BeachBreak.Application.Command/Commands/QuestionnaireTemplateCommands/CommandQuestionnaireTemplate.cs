using ti8m.BeachBreak.Core.Domain;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class CommandQuestionnaireTemplate
{
    public Guid Id { get; set; }
    public string NameGerman { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public string DescriptionGerman { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public QuestionnaireProcessType ProcessType { get; set; } = QuestionnaireProcessType.PerformanceReview;
    public bool IsCustomizable { get; set; } = false;
    public bool AutoInitialize { get; set; } = false;
    public List<CommandQuestionSection> Sections { get; set; } = new();
}