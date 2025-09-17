namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class PublishQuestionnaireTemplateCommand : ICommand<Result>
{
    public Guid Id { get; init; }
    public string PublishedBy { get; init; }

    public PublishQuestionnaireTemplateCommand(Guid id, string publishedBy)
    {
        Id = id;
        PublishedBy = publishedBy;
    }
}