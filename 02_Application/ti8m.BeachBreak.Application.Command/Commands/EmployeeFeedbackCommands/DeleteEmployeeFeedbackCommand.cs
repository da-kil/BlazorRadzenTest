namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Command to soft delete employee feedback.
/// Maintains audit trail by marking feedback as deleted rather than removing it.
/// </summary>
public class DeleteEmployeeFeedbackCommand : ICommand<Result>
{
    /// <summary>
    /// ID of the feedback to delete.
    /// </summary>
    public Guid FeedbackId { get; set; }

    /// <summary>
    /// Optional reason for deletion (for audit purposes).
    /// </summary>
    public string? DeleteReason { get; set; }

    public DeleteEmployeeFeedbackCommand() { }

    public DeleteEmployeeFeedbackCommand(Guid feedbackId, string? deleteReason = null)
    {
        FeedbackId = feedbackId;
        DeleteReason = deleteReason;
    }
}