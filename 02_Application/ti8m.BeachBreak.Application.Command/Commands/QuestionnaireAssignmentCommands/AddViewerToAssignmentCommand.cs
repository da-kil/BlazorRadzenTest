namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to add a viewer to a questionnaire assignment.
/// Viewers have read-only access to the assignment for collaboration, mentoring, or oversight.
/// Only HR/Admin roles can add viewers.
/// </summary>
public class AddViewerToAssignmentCommand : ICommand<Result>
{
    public Guid AssignmentId { get; init; }
    public Guid ViewerEmployeeId { get; init; }
    public Guid AddedByUserId { get; init; }

    public AddViewerToAssignmentCommand(Guid assignmentId, Guid viewerEmployeeId, Guid addedByUserId)
    {
        AssignmentId = assignmentId;
        ViewerEmployeeId = viewerEmployeeId;
        AddedByUserId = addedByUserId;
    }
}