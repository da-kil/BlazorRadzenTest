namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Query to get available predecessor questionnaires that can be linked for goal rating.
/// Returns finalized questionnaires for the same employee, same category, that have goals.
/// Validates that the assignment belongs to the requesting user for security.
/// </summary>
public class GetAvailablePredecessorsQuery : IQuery<Result<IEnumerable<AvailablePredecessorDto>>>
{
    public Guid AssignmentId { get; init; }
    public Guid QuestionId { get; init; }
    public Guid RequestingUserId { get; init; }

    public GetAvailablePredecessorsQuery(Guid assignmentId, Guid questionId, Guid requestingUserId)
    {
        AssignmentId = assignmentId;
        QuestionId = questionId;
        RequestingUserId = requestingUserId;
    }
}
