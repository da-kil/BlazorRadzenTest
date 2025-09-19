namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeProgressQuery : IQuery<Result<IEnumerable<ProgressQueries.AssignmentProgress>>>
{
    public Guid EmployeeId { get; set; }

    public EmployeeProgressQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
}