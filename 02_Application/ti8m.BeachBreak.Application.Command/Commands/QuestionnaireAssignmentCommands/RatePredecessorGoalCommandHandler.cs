using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Mappers;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles rating a goal from a predecessor questionnaire.
/// Employee and Manager can rate separately during their respective in-progress states.
/// </summary>
public class RatePredecessorGoalCommandHandler
    : ICommandHandler<RatePredecessorGoalCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<RatePredecessorGoalCommandHandler> logger;

    public RatePredecessorGoalCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<RatePredecessorGoalCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(RatePredecessorGoalCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Rating predecessor goal {SourceGoalId} from assignment {SourceAssignmentId} for assignment {AssignmentId} by role {Role}",
                command.SourceGoalId, command.SourceAssignmentId, command.AssignmentId, command.RatedByRole);

            // Load current assignment
            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId, cancellationToken: cancellationToken);

            // Predecessor goal data is now passed in the command (read from QuestionnaireResponse by the caller)
            // No need to load predecessor assignment since goals are stored in QuestionnaireResponse
            assignment.RatePredecessorGoal(
                command.QuestionId,
                command.SourceAssignmentId,
                command.SourceGoalId,
                command.PredecessorGoalData,
                ApplicationRoleMapper.MapToDomain(command.RatedByRole),
                command.DegreeOfAchievement,
                command.Justification,
                command.RatedByEmployeeId);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation(
                "Successfully rated predecessor goal {SourceGoalId} for assignment {AssignmentId}",
                command.SourceGoalId, command.AssignmentId);

            return Result.Success("Predecessor goal rated successfully");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex,
                "Business rule violation when rating predecessor goal {SourceGoalId} for assignment {AssignmentId}",
                command.SourceGoalId, command.AssignmentId);
            return Result.Fail(ex.Message, 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error rating predecessor goal {SourceGoalId} for assignment {AssignmentId}",
                command.SourceGoalId, command.AssignmentId);
            return Result.Fail("Failed to rate predecessor goal: " + ex.Message, 500);
        }
    }
}
