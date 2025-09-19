namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class ManagerTeamAssignmentsQuery : IQuery<Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>>
{
    public Guid ManagerId { get; set; }

    public ManagerTeamAssignmentsQuery(Guid managerId)
    {
        ManagerId = managerId;
    }
}