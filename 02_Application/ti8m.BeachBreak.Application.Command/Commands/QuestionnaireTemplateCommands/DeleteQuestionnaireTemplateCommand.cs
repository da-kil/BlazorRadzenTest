namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class DeleteQuestionnaireTemplateCommand : ICommand<Result>
{
    public Guid Id { get; init; }

    public DeleteQuestionnaireTemplateCommand(Guid id)
    {
        Id = id;
    }
}
