using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the extension of a questionnaire assignment's due date.
/// </summary>
public class ExtendAssignmentDueDateCommandHandler
    : ICommandHandler<ExtendAssignmentDueDateCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<ExtendAssignmentDueDateCommandHandler> logger;

    public ExtendAssignmentDueDateCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<ExtendAssignmentDueDateCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(ExtendAssignmentDueDateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Extending due date for assignment {AssignmentId} to {NewDueDate}",
                command.AssignmentId, command.NewDueDate);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.ExtendDueDate(command.NewDueDate, command.ExtensionReason);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully extended due date for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Assignment due date extended");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extending due date for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to extend assignment due date: " + ex.Message, 500);
        }
    }
}
