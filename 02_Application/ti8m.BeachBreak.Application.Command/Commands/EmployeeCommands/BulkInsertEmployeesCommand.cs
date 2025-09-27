namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

public class BulkInsertEmployeesCommand : ICommand<Result>
{
    public IEnumerable<SyncEmployee> Employees { get; init; }

    public BulkInsertEmployeesCommand(IEnumerable<SyncEmployee> employees)
    {
        Employees = employees ?? throw new ArgumentNullException(nameof(employees));
    }
}