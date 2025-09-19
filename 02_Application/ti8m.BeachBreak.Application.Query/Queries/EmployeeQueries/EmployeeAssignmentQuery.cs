namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeAssignmentQuery : IQuery<Result<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>
{
    public Guid EmployeeId { get; set; }
    public Guid AssignmentId { get; set; }

    public EmployeeAssignmentQuery(Guid employeeId, Guid assignmentId)
    {
        EmployeeId = employeeId;
        AssignmentId = assignmentId;
    }
}