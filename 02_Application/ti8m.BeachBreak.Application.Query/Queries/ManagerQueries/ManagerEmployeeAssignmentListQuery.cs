namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class ManagerEmployeeAssignmentListQuery : IQuery<Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>>
{
    public Guid ManagerId { get; set; }
    public Guid EmployeeId { get; set; }

    public ManagerEmployeeAssignmentListQuery(Guid managerId, Guid employeeId)
    {
        ManagerId = managerId;
        EmployeeId = employeeId;
    }
}