using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the completion of a review meeting by the manager.
/// </summary>
public class FinishReviewMeetingCommandHandler
    : ICommandHandler<FinishReviewMeetingCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<FinishReviewMeetingCommandHandler> logger;

    public FinishReviewMeetingCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<FinishReviewMeetingCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(FinishReviewMeetingCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Manager finishing review meeting for assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                command.ExpectedVersion,
                cancellationToken);
            assignment.FinishReviewMeeting(command.FinishedByEmployeeId, command.ReviewSummary);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully finished review meeting for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Review meeting finished");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finishing review meeting for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to finish review meeting: " + ex.Message, 500);
        }
    }
}
