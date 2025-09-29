namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class WithdrawAssignmentCommand : ICommand<Result>
{
    public Guid AssignmentId { get; init; }
    public string WithdrawnBy { get; init; }
    public string? WithdrawalReason { get; init; }

    public WithdrawAssignmentCommand(Guid assignmentId, string withdrawnBy, string? withdrawalReason = null)
    {
        AssignmentId = assignmentId;
        WithdrawnBy = withdrawnBy;
        WithdrawalReason = withdrawalReason;
    }
}