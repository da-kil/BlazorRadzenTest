namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

public class BulkInsertEmployeesCommand : ICommand<Result>
{
    public IEnumerable<Employee> Employees { get; init; }

    public BulkInsertEmployeesCommand(IEnumerable<Employee> employees)
    {
        Employees = employees ?? throw new ArgumentNullException(nameof(employees));
    }
}