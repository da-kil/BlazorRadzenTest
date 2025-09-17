namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class DeactivateQuestionnaireTemplateCommand : ICommand<Result>
{
    public Guid Id { get; init; }

    public DeactivateQuestionnaireTemplateCommand(Guid id)
    {
        Id = id;
    }
}