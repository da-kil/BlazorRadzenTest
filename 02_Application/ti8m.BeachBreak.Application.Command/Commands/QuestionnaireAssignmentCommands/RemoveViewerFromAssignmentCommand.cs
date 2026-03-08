namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to remove a viewer from a questionnaire assignment.
/// This revokes the viewer's read-only access to the assignment.
/// Only HR/Admin roles can remove viewers.
/// </summary>
public class RemoveViewerFromAssignmentCommand : ICommand<Result>
{
    public Guid AssignmentId { get; init; }
    public Guid ViewerEmployeeId { get; init; }
    public Guid RemovedByUserId { get; init; }

    public RemoveViewerFromAssignmentCommand(Guid assignmentId, Guid viewerEmployeeId, Guid removedByUserId)
    {
        AssignmentId = assignmentId;
        ViewerEmployeeId = viewerEmployeeId;
        RemovedByUserId = removedByUserId;
    }
}