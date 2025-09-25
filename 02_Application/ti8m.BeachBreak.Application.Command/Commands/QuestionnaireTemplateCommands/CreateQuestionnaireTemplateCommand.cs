namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class CreateQuestionnaireTemplateCommand : ICommand<Result>
{
    public CommandQuestionnaireTemplate QuestionnaireTemplate { get; init; }

    public CreateQuestionnaireTemplateCommand(CommandQuestionnaireTemplate questionnaireTemplate)
    {
        QuestionnaireTemplate = questionnaireTemplate;
    }
}
