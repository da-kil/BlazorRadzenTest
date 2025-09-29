namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

// Todo: check if really necessary, Bulk insert should handle all scenarios
public class BulkUpdateEmployeesCommand : ICommand<Result>
{
    public IEnumerable<SyncEmployee> Employees { get; init; }

    public BulkUpdateEmployeesCommand(IEnumerable<SyncEmployee> employees)
    {
        Employees = employees ?? throw new ArgumentNullException(nameof(employees));
    }
}