namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class SendAssignmentReminderCommand : ICommand<Result>
{
    public Guid AssignmentId { get; set; }
    public string Message { get; set; }
    public Guid SentByEmployeeId { get; set; }

    public SendAssignmentReminderCommand(Guid assignmentId, string message, Guid sentByEmployeeId)
    {
        AssignmentId = assignmentId;
        Message = message;
        SentByEmployeeId = sentByEmployeeId;
    }
}