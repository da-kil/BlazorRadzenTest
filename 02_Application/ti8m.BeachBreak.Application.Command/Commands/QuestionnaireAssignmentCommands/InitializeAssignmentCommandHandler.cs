using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the initialization of a questionnaire assignment.
/// Transitions assignment from Assigned to Initialized state.
/// </summary>
public class InitializeAssignmentCommandHandler
    : ICommandHandler<InitializeAssignmentCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<InitializeAssignmentCommandHandler> logger;

    public InitializeAssignmentCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<InitializeAssignmentCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(
        InitializeAssignmentCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInitializeAssignment(command.AssignmentId, command.InitializedByEmployeeId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                cancellationToken: cancellationToken);

            assignment.StartInitialization(
                command.InitializedByEmployeeId,
                command.InitializationNotes);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogAssignmentInitialized(command.AssignmentId, command.InitializedByEmployeeId);
            return Result.Success("Assignment initialized successfully");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInitializeAssignmentFailed(command.AssignmentId, ex.Message, ex);
            return Result.Fail($"Cannot initialize assignment: {ex.Message}", 400);
        }
        catch (Exception ex)
        {
            logger.LogInitializeAssignmentFailed(command.AssignmentId, ex.Message, ex);
            return Result.Fail($"Failed to initialize assignment: {ex.Message}", 500);
        }
    }
}
