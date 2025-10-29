using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles adding a new goal to a questionnaire assignment during in-progress states.
/// </summary>
public class AddGoalCommandHandler
    : ICommandHandler<AddGoalCommand, Result<Guid>>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<AddGoalCommandHandler> logger;

    public AddGoalCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<AddGoalCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result<Guid>> HandleAsync(AddGoalCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Adding goal to assignment {AssignmentId} for question {QuestionId} by role {Role}",
                command.AssignmentId, command.QuestionId, command.AddedByRole);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId, cancellationToken: cancellationToken);

            var goalId = Guid.NewGuid();

            // Default weighting to 0 if not provided (will be set during InReview by manager)
            var weighting = command.WeightingPercentage ?? 0m;

            assignment.AddGoal(
                command.QuestionId,
                goalId,
                command.AddedByRole,
                command.TimeframeFrom,
                command.TimeframeTo,
                command.ObjectiveDescription,
                command.MeasurementMetric,
                weighting,
                command.AddedByEmployeeId);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation(
                "Successfully added goal {GoalId} to assignment {AssignmentId}",
                goalId, command.AssignmentId);

            return Result<Guid>.Success(goalId, 201); // 201 Created
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation when adding goal to assignment {AssignmentId}", command.AssignmentId);
            return Result<Guid>.Fail(ex.Message, 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding goal to assignment {AssignmentId}", command.AssignmentId);
            return Result<Guid>.Fail("Failed to add goal: " + ex.Message, 500);
        }
    }
}
