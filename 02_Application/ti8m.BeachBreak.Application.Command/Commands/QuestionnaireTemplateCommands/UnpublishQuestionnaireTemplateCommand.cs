namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class UnpublishQuestionnaireTemplateCommand : ICommand<Result>
{
    public Guid Id { get; init; }

    public UnpublishQuestionnaireTemplateCommand(Guid id)
    {
        Id = id;
    }
}