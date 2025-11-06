using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Mappers;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles modification of a predecessor goal rating during review meeting.
/// </summary>
public class ModifyPredecessorGoalRatingCommandHandler
    : ICommandHandler<ModifyPredecessorGoalRatingCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<ModifyPredecessorGoalRatingCommandHandler> logger;

    public ModifyPredecessorGoalRatingCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<ModifyPredecessorGoalRatingCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(ModifyPredecessorGoalRatingCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Modifying predecessor goal rating for goal {SourceGoalId} in assignment {AssignmentId} by role {Role}",
                command.SourceGoalId, command.AssignmentId, command.ModifiedByRole);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId, cancellationToken: cancellationToken);

            assignment.ModifyPredecessorGoalRating(
                command.SourceGoalId,
                ApplicationRoleMapper.MapToDomain(command.ModifiedByRole),
                command.DegreeOfAchievement,
                command.Justification,
                command.ChangeReason,
                command.ModifiedByEmployeeId);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation(
                "Successfully modified predecessor goal rating for goal {SourceGoalId} in assignment {AssignmentId}",
                command.SourceGoalId, command.AssignmentId);

            return Result.Success("Predecessor goal rating modified successfully");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex,
                "Business rule violation when modifying predecessor goal rating for goal {SourceGoalId} in assignment {AssignmentId}",
                command.SourceGoalId, command.AssignmentId);
            return Result.Fail(ex.Message, 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error modifying predecessor goal rating for goal {SourceGoalId} in assignment {AssignmentId}",
                command.SourceGoalId, command.AssignmentId);
            return Result.Fail("Failed to modify predecessor goal rating: " + ex.Message, 500);
        }
    }
}
