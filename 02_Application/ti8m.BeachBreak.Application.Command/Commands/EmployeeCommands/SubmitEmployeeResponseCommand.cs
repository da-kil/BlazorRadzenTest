namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

public class SubmitEmployeeResponseCommand : ICommand<Result>
{
    public Guid EmployeeId { get; set; }
    public Guid AssignmentId { get; set; }

    public SubmitEmployeeResponseCommand(Guid employeeId, Guid assignmentId)
    {
        EmployeeId = employeeId;
        AssignmentId = assignmentId;
    }
}