using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class GetTeamAssignmentsQuery : IQuery<Result<IEnumerable<QuestionnaireAssignment>>>
{
    public Guid ManagerId { get; }
    public AssignmentStatus? FilterByStatus { get; }

    public GetTeamAssignmentsQuery(Guid managerId, AssignmentStatus? filterByStatus = null)
    {
        ManagerId = managerId;
        FilterByStatus = filterByStatus;
    }
}
