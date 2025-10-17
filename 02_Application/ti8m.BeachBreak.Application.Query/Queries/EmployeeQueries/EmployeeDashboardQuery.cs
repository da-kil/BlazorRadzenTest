namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeDashboardQuery : IQuery<Result<EmployeeDashboard?>>
{
    public Guid EmployeeId { get; init; }

    public EmployeeDashboardQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
}
