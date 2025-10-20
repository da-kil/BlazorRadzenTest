using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the withdrawal of a questionnaire assignment.
/// </summary>
public class WithdrawAssignmentCommandHandler
    : ICommandHandler<WithdrawAssignmentCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<WithdrawAssignmentCommandHandler> logger;

    public WithdrawAssignmentCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<WithdrawAssignmentCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(WithdrawAssignmentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Withdrawing assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.Withdraw(command.WithdrawnByEmployeeId, command.WithdrawalReason);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully withdrew assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Assignment withdrawn");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error withdrawing assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to withdraw assignment: " + ex.Message, 500);
        }
    }
}
