namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class ArchiveQuestionnaireTemplateCommand : ICommand<Result>
{
    public Guid Id { get; init; }

    public ArchiveQuestionnaireTemplateCommand(Guid id)
    {
        Id = id;
    }
}