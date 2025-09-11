namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class CreateQuestionnaireTemplateCommand : ICommand<Result>
{
    public QuestionnaireTemplate QuestionnaireTemplate { get; init; }

    public CreateQuestionnaireTemplateCommand(QuestionnaireTemplate questionnaireTemplate)
    {
        QuestionnaireTemplate = questionnaireTemplate;
    }
}
