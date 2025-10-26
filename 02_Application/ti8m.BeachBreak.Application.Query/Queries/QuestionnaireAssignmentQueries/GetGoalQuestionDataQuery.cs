using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

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
    public CompletionRole CurrentUserRole { get; init; }

    public GetGoalQuestionDataQuery(Guid assignmentId, Guid questionId, CompletionRole currentUserRole)
    {
        AssignmentId = assignmentId;
        QuestionId = questionId;
        CurrentUserRole = currentUserRole;
    }
}
