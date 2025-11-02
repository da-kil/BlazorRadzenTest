using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Query to get all goal data for a specific question within an assignment.
/// Includes goals added by Employee/Manager and ratings of predecessor goals.
/// Filters goals based on workflow state and current user role.
/// </summary>
public class GetGoalQuestionDataQuery : IQuery<Result<GoalQuestionDataDto>>
{
    public Guid AssignmentId { get; init; }
    public Guid QuestionId { get; init; }
    public ApplicationRole CurrentUserRole { get; init; }

    public GetGoalQuestionDataQuery(Guid assignmentId, Guid questionId, ApplicationRole currentUserRole)
    {
        AssignmentId = assignmentId;
        QuestionId = questionId;
        CurrentUserRole = currentUserRole;
    }
}
