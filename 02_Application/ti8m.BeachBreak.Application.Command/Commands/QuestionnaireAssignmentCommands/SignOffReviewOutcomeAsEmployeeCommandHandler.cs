using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the employee's sign-off on review outcome.
/// This is the intermediate step after manager finishes review meeting.
/// </summary>
public class SignOffReviewOutcomeAsEmployeeCommandHandler
    : ICommandHandler<SignOffReviewOutcomeAsEmployeeCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<SignOffReviewOutcomeAsEmployeeCommandHandler> logger;

    public SignOffReviewOutcomeAsEmployeeCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<SignOffReviewOutcomeAsEmployeeCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(SignOffReviewOutcomeAsEmployeeCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Employee signing-off review outcome for assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                command.ExpectedVersion,
                cancellationToken);

            logger.LogInformation("Assignment {AssignmentId} loaded with workflow state: {WorkflowState}, IsLocked: {IsLocked}",
                command.AssignmentId, assignment.WorkflowState, assignment.IsLocked);

            assignment.SignOffReviewOutcome(command.SignedOffByEmployeeId, command.SignOffComments);

            logger.LogInformation("After SignOffReviewOutcome, workflow state: {WorkflowState}", assignment.WorkflowState);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully signed-off review outcome for assignment {AssignmentId}, new state: {WorkflowState}",
                command.AssignmentId, assignment.WorkflowState);
            return Result.Success("Review outcome signed-off");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error signing-off review outcome for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to sign-off review outcome: " + ex.Message, 500);
        }
    }
}