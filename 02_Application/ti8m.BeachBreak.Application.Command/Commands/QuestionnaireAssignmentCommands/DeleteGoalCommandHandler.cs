using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles deleting an existing goal from a questionnaire assignment during in-progress states.
/// </summary>
public class DeleteGoalCommandHandler
    : ICommandHandler<DeleteGoalCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<DeleteGoalCommandHandler> logger;

    public DeleteGoalCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<DeleteGoalCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(DeleteGoalCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Deleting goal {GoalId} from assignment {AssignmentId}",
                command.GoalId, command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId, cancellationToken: cancellationToken);

            assignment.DeleteGoal(
                command.GoalId,
                command.DeletedByEmployeeId);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation(
                "Successfully deleted goal {GoalId} from assignment {AssignmentId}",
                command.GoalId, command.AssignmentId);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation when deleting goal {GoalId} from assignment {AssignmentId}",
                command.GoalId, command.AssignmentId);
            return Result.Fail(ex.Message, 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting goal {GoalId} from assignment {AssignmentId}",
                command.GoalId, command.AssignmentId);
            return Result.Fail("Failed to delete goal: " + ex.Message, 500);
        }
    }
}
