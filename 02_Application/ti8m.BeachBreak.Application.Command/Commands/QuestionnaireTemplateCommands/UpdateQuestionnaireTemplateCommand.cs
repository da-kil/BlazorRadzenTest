namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class UpdateQuestionnaireTemplateCommand : ICommand<Result>
{
    public Guid Id { get; init; }
    public QuestionnaireTemplate QuestionnaireTemplate { get; init; }

    public UpdateQuestionnaireTemplateCommand(Guid id, QuestionnaireTemplate questionnaireTemplate)
    {
        Id = id;
        QuestionnaireTemplate = questionnaireTemplate;
    }
}
