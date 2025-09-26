namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class UpdateQuestionnaireTemplateCommand : ICommand<Result>
{
    public Guid Id { get; init; }
    public CommandQuestionnaireTemplate QuestionnaireTemplate { get; init; }

    public UpdateQuestionnaireTemplateCommand(Guid id, CommandQuestionnaireTemplate questionnaireTemplate)
    {
        Id = id;
        QuestionnaireTemplate = questionnaireTemplate;
    }
}
