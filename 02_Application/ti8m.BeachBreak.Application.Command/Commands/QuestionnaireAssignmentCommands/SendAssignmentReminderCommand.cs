namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class SendAssignmentReminderCommand : ICommand<Result>
{
    public Guid AssignmentId { get; set; }
    public string Message { get; set; }
    public string SentBy { get; set; }

    public SendAssignmentReminderCommand(Guid assignmentId, string message, string sentBy)
    {
        AssignmentId = assignmentId;
        Message = message;
        SentBy = sentBy;
    }
}