namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeAssignmentListQuery : IQuery<Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>>
{
    public Guid EmployeeId { get; set; }

    public EmployeeAssignmentListQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
}