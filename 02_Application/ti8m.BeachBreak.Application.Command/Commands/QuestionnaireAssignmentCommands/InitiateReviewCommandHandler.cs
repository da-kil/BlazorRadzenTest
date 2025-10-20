using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the initiation of a review meeting for a questionnaire assignment.
/// Transitions the assignment into the review phase where manager and employee discuss responses.
/// </summary>
public class InitiateReviewCommandHandler
    : ICommandHandler<InitiateReviewCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<InitiateReviewCommandHandler> logger;

    public InitiateReviewCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<InitiateReviewCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(InitiateReviewCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Initiating review for assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.InitiateReview(command.InitiatedByEmployeeId);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully initiated review for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Review initiated");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initiating review for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to initiate review: " + ex.Message, 500);
        }
    }
}
