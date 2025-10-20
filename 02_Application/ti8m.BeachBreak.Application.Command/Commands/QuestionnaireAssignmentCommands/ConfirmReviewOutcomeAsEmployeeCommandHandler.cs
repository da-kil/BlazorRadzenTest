using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the employee's confirmation of the review outcome.
/// </summary>
public class ConfirmReviewOutcomeAsEmployeeCommandHandler
    : ICommandHandler<ConfirmReviewOutcomeAsEmployeeCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<ConfirmReviewOutcomeAsEmployeeCommandHandler> logger;

    public ConfirmReviewOutcomeAsEmployeeCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<ConfirmReviewOutcomeAsEmployeeCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(ConfirmReviewOutcomeAsEmployeeCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Employee confirming review outcome for assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                command.ExpectedVersion,
                cancellationToken);

            logger.LogInformation("Assignment {AssignmentId} loaded with workflow state: {WorkflowState}, IsLocked: {IsLocked}",
                command.AssignmentId, assignment.WorkflowState, assignment.IsLocked);

            assignment.ConfirmReviewOutcomeAsEmployee(command.ConfirmedByEmployeeId, command.EmployeeComments);

            logger.LogInformation("After ConfirmReviewOutcomeAsEmployee, workflow state: {WorkflowState}", assignment.WorkflowState);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully confirmed review outcome for assignment {AssignmentId}, new state: {WorkflowState}",
                command.AssignmentId, assignment.WorkflowState);
            return Result.Success("Review outcome confirmed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming review outcome for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to confirm review outcome: " + ex.Message, 500);
        }
    }
}
