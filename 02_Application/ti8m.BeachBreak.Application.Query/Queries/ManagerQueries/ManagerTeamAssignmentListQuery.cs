namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class ManagerTeamAssignmentListQuery : IQuery<Result<IEnumerable<QuestionnaireAssignmentQueries.QuestionnaireAssignment>>>
{
    public Guid ManagerId { get; set; }

    public ManagerTeamAssignmentListQuery(Guid managerId)
    {
        ManagerId = managerId;
    }
}