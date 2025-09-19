namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeResponseQuery : IQuery<Result<ResponseQueries.QuestionnaireResponse>>
{
    public Guid EmployeeId { get; set; }
    public Guid AssignmentId { get; set; }

    public EmployeeResponseQuery(Guid employeeId, Guid assignmentId)
    {
        EmployeeId = employeeId;
        AssignmentId = assignmentId;
    }
}