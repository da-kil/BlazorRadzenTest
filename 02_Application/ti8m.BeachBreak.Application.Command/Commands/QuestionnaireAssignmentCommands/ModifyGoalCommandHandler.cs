using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Mappers;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles modification of an existing goal during review meeting.
/// </summary>
public class ModifyGoalCommandHandler
    : ICommandHandler<ModifyGoalCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<ModifyGoalCommandHandler> logger;

    public ModifyGoalCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<ModifyGoalCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(ModifyGoalCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Modifying goal {GoalId} for assignment {AssignmentId} by role {Role}",
                command.GoalId, command.AssignmentId, command.ModifiedByRole);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId, cancellationToken: cancellationToken);

            assignment.ModifyGoal(
                command.GoalId,
                command.TimeframeFrom,
                command.TimeframeTo,
                command.ObjectiveDescription,
                command.MeasurementMetric,
                command.WeightingPercentage,
                ApplicationRoleMapper.MapToDomain(command.ModifiedByRole),
                command.ChangeReason,
                command.ModifiedByEmployeeId);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation(
                "Successfully modified goal {GoalId} for assignment {AssignmentId}",
                command.GoalId, command.AssignmentId);

            return Result.Success("Goal modified successfully");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation when modifying goal {GoalId} for assignment {AssignmentId}",
                command.GoalId, command.AssignmentId);
            return Result.Fail(ex.Message, 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error modifying goal {GoalId} for assignment {AssignmentId}",
                command.GoalId, command.AssignmentId);
            return Result.Fail("Failed to modify goal: " + ex.Message, 500);
        }
    }
}
