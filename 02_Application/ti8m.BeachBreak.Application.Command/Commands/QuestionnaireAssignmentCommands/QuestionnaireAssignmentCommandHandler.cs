namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class QuestionnaireAssignmentCommandHandler : ICommandHandler<CreateQuestionnaireAssignmentCommand, Result>
{
    public Task<Result> HandleAsync(CreateQuestionnaireAssignmentCommand command, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Success());
    }
}
