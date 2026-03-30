namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class QuestionnaireAssignmentsAsViewerQuery : IQuery<Result<IEnumerable<QuestionnaireAssignment>>>
{
    public Guid ViewerEmployeeId { get; init; }
}
