using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the initiation of work on a questionnaire assignment.
/// </summary>
public class StartAssignmentWorkCommandHandler
    : ICommandHandler<StartAssignmentWorkCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<StartAssignmentWorkCommandHandler> logger;

    public StartAssignmentWorkCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<StartAssignmentWorkCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(StartAssignmentWorkCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting work on assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.StartWork();
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully started work on assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Assignment work started");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting work on assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to start assignment work: " + ex.Message, 500);
        }
    }
}
