namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Query to get available predecessor questionnaires that can be linked for goal rating.
/// Returns questionnaires for the same employee, same category, that have goals.
/// </summary>
public class GetAvailablePredecessorsQuery : IQuery<Result<IEnumerable<AvailablePredecessorDto>>>
{
    public Guid AssignmentId { get; init; }
    public Guid QuestionId { get; init; }

    public GetAvailablePredecessorsQuery(Guid assignmentId, Guid questionId)
    {
        AssignmentId = assignmentId;
        QuestionId = questionId;
    }
}
