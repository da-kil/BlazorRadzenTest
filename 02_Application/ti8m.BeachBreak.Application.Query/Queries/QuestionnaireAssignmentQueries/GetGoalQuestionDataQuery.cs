namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Query to get all goal data for a specific question within an assignment.
/// Includes goals added by Employee/Manager and ratings of predecessor goals.
/// </summary>
public class GetGoalQuestionDataQuery : IQuery<Result<GoalQuestionDataDto>>
{
    public Guid AssignmentId { get; init; }
    public Guid QuestionId { get; init; }

    public GetGoalQuestionDataQuery(Guid assignmentId, Guid questionId)
    {
        AssignmentId = assignmentId;
        QuestionId = questionId;
    }
}
