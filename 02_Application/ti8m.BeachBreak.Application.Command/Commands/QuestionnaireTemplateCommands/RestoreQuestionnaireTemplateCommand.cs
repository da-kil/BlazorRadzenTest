namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class RestoreQuestionnaireTemplateCommand : ICommand<Result>
{
    public Guid Id { get; init; }

    public RestoreQuestionnaireTemplateCommand(Guid id)
    {
        Id = id;
    }
}