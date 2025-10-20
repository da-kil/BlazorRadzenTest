using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the completion of work on a questionnaire assignment.
/// </summary>
public class CompleteAssignmentWorkCommandHandler
    : ICommandHandler<CompleteAssignmentWorkCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<CompleteAssignmentWorkCommandHandler> logger;

    public CompleteAssignmentWorkCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<CompleteAssignmentWorkCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(CompleteAssignmentWorkCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Completing work on assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.CompleteWork();
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully completed work on assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Assignment work completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing work on assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to complete assignment work: " + ex.Message, 500);
        }
    }
}
