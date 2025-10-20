using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the final approval and closure of a questionnaire by the manager.
/// </summary>
public class FinalizeQuestionnaireAsManagerCommandHandler
    : ICommandHandler<FinalizeQuestionnaireAsManagerCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<FinalizeQuestionnaireAsManagerCommandHandler> logger;

    public FinalizeQuestionnaireAsManagerCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<FinalizeQuestionnaireAsManagerCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(FinalizeQuestionnaireAsManagerCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Manager finalizing questionnaire for assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                command.ExpectedVersion,
                cancellationToken);
            assignment.FinalizeAsManager(command.FinalizedByEmployeeId, command.ManagerFinalNotes);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully finalized questionnaire for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Questionnaire finalized");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finalizing questionnaire for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to finalize questionnaire: " + ex.Message, 500);
        }
    }
}
