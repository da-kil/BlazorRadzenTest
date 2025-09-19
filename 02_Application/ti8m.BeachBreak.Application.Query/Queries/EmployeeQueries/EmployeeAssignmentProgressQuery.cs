namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeAssignmentProgressQuery : IQuery<Result<IEnumerable<ProgressQueries.AssignmentProgress>>>
{
    public Guid EmployeeId { get; set; }
    public Guid? AssignmentId { get; set; }

    public EmployeeAssignmentProgressQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }

    public EmployeeAssignmentProgressQuery(Guid employeeId, Guid assignmentId)
    {
        EmployeeId = employeeId;
        AssignmentId = assignmentId;
    }
}