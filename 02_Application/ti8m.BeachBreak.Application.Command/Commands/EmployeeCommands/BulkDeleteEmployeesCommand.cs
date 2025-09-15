namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

public class BulkDeleteEmployeesCommand : ICommand<Result>
{
    public IEnumerable<Guid> EmployeeIds { get; init; }

    public BulkDeleteEmployeesCommand(IEnumerable<Guid> employeeIds)
    {
        EmployeeIds = employeeIds ?? throw new ArgumentNullException(nameof(employeeIds));
    }
}