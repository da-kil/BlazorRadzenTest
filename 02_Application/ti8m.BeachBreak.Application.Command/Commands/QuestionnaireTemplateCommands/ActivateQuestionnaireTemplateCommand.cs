namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class ActivateQuestionnaireTemplateCommand : ICommand<Result>
{
    public Guid Id { get; init; }

    public ActivateQuestionnaireTemplateCommand(Guid id)
    {
        Id = id;
    }
}