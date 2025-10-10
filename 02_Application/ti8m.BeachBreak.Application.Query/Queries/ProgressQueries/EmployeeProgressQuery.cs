namespace ti8m.BeachBreak.Application.Query.Queries.ProgressQueries;

public class EmployeeProgressQuery : IQuery<Result<IEnumerable<AssignmentProgress>>>
{
    public Guid EmployeeId { get; init; }

    public EmployeeProgressQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
}
