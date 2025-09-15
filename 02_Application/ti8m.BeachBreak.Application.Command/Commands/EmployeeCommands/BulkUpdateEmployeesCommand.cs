namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

public class BulkUpdateEmployeesCommand : ICommand<Result>
{
    public IEnumerable<Employee> Employees { get; init; }

    public BulkUpdateEmployeesCommand(IEnumerable<Employee> employees)
    {
        Employees = employees ?? throw new ArgumentNullException(nameof(employees));
    }
}