using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;
using QuestionnaireAssignmentDto = ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries.QuestionnaireAssignment;

namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class GetTeamAssignmentsQuery : IQuery<Result<IEnumerable<QuestionnaireAssignmentDto>>>
{
    public Guid ManagerId { get; }
    public WorkflowState? FilterByWorkflowState { get; }

    public GetTeamAssignmentsQuery(Guid managerId, WorkflowState? filterByWorkflowState = null)
    {
        ManagerId = managerId;
        FilterByWorkflowState = filterByWorkflowState;
    }
}
