namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class WithdrawAssignmentCommand : ICommand<Result>
{
    public Guid AssignmentId { get; init; }
    public Guid WithdrawnByEmployeeId { get; init; }
    public string? WithdrawalReason { get; init; }

    public WithdrawAssignmentCommand(Guid assignmentId, Guid withdrawnByEmployeeId, string? withdrawalReason = null)
    {
        AssignmentId = assignmentId;
        WithdrawnByEmployeeId = withdrawnByEmployeeId;
        WithdrawalReason = withdrawalReason;
    }
}