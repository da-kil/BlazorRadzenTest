namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class ExtendAssignmentDueDateCommand : ICommand<Result>
{
    public Guid AssignmentId { get; init; }
    public DateTime NewDueDate { get; init; }
    public string? ExtensionReason { get; init; }

    public ExtendAssignmentDueDateCommand(Guid assignmentId, DateTime newDueDate, string? extensionReason = null)
    {
        AssignmentId = assignmentId;
        NewDueDate = newDueDate;
        ExtensionReason = extensionReason;
    }
}