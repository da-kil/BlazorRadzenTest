namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class QuestionnaireEmployeeAssignmentListQuery : IQuery<Result<IEnumerable<QuestionnaireAssignment>>>
{
    public Guid EmployeeId { get; init; }

    public QuestionnaireEmployeeAssignmentListQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
}