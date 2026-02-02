using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Mappers;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles editing of specific goals during a review meeting.
/// Dedicated handler for goal editing separate from generic answer editing.
/// </summary>
public class EditGoalDuringReviewCommandHandler
    : ICommandHandler<EditGoalDuringReviewCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly IQuestionnaireResponseAggregateRepository responseRepository;
    private readonly ILogger<EditGoalDuringReviewCommandHandler> logger;

    public EditGoalDuringReviewCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        IQuestionnaireResponseAggregateRepository responseRepository,
        ILogger<EditGoalDuringReviewCommandHandler> logger)
    {
        this.repository = repository;
        this.responseRepository = responseRepository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(EditGoalDuringReviewCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting goal edit for assignment {AssignmentId}, goal {GoalId}",
                command.AssignmentId, command.GoalId);

            // 1. Load response aggregate (where goal data lives)
            var response = await responseRepository.FindByAssignmentIdAsync(command.AssignmentId, cancellationToken);
            if (response == null)
            {
                logger.LogError("QuestionnaireResponse not found for assignment {AssignmentId}", command.AssignmentId);
                return Result.Fail("Response not found", 404);
            }

            // 2. Let the DOMAIN handle the complexity
            response.EditGoal(
                command.SectionId,
                command.GoalId,
                command.ObjectiveDescription,
                command.MeasurementMetric,
                command.TimeframeFrom,
                command.TimeframeTo,
                command.WeightingPercentage,
                ApplicationRoleMapper.MapToDomain(command.OriginalCompletionRole),
                command.EditedByEmployeeId);

            // 3. Store and done
            await responseRepository.StoreAsync(response, cancellationToken);

            logger.LogInformation("Successfully edited goal {GoalId} for assignment {AssignmentId}",
                command.GoalId, command.AssignmentId);

            return Result.Success("Goal edited successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error editing goal {GoalId} for assignment {AssignmentId}",
                command.GoalId, command.AssignmentId);
            return Result.Fail("Failed to edit goal: " + ex.Message, 500);
        }
    }
}