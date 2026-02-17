using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles removing a viewer from a questionnaire assignment.
/// This revokes the viewer's read-only access to the assignment.
/// </summary>
public class RemoveViewerFromAssignmentCommandHandler
    : ICommandHandler<RemoveViewerFromAssignmentCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository assignmentRepository;
    private readonly ILogger<RemoveViewerFromAssignmentCommandHandler> logger;

    public RemoveViewerFromAssignmentCommandHandler(
        IQuestionnaireAssignmentAggregateRepository assignmentRepository,
        ILogger<RemoveViewerFromAssignmentCommandHandler> logger)
    {
        this.assignmentRepository = assignmentRepository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(RemoveViewerFromAssignmentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Removing viewer {ViewerEmployeeId} from assignment {AssignmentId} by user {RemovedByUserId}",
                command.ViewerEmployeeId,
                command.AssignmentId,
                command.RemovedByUserId);

            // Load the assignment aggregate
            var assignment = await assignmentRepository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                cancellationToken: cancellationToken);

            // Remove the viewer from the assignment
            assignment.RemoveViewer(command.ViewerEmployeeId, command.RemovedByUserId);

            // Store the updated assignment
            await assignmentRepository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation(
                "Successfully removed viewer {ViewerEmployeeId} from assignment {AssignmentId}",
                command.ViewerEmployeeId,
                command.AssignmentId);

            return Result.Success("Viewer removed from assignment successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error removing viewer {ViewerEmployeeId} from assignment {AssignmentId}",
                command.ViewerEmployeeId,
                command.AssignmentId);

            return Result.Fail("Failed to remove viewer: " + ex.Message, 500);
        }
    }
}