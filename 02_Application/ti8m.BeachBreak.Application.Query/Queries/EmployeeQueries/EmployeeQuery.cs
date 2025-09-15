namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeQuery : IQuery<Result<Employee?>>
{
    public Guid EmployeeId { get; init; }

    public EmployeeQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
}