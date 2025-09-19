namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class SendBulkAssignmentReminderCommand : ICommand<Result>
{
    public IEnumerable<Guid> AssignmentIds { get; set; }
    public string Message { get; set; }
    public string SentBy { get; set; }

    public SendBulkAssignmentReminderCommand(IEnumerable<Guid> assignmentIds, string message, string sentBy)
    {
        AssignmentIds = assignmentIds;
        Message = message;
        SentBy = sentBy;
    }
}