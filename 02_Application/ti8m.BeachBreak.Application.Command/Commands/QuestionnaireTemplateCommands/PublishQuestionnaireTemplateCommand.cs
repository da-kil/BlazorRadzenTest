namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class PublishQuestionnaireTemplateCommand : ICommand<Result>
{
    public Guid Id { get; init; }
    public Guid PublishedByEmployeeId { get; init; }

    public PublishQuestionnaireTemplateCommand(Guid id, Guid publishedByEmployeeId)
    {
        Id = id;
        PublishedByEmployeeId = publishedByEmployeeId;
    }
}