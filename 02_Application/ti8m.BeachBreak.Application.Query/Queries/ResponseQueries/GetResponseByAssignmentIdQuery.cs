namespace ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;

public class GetResponseByAssignmentIdQuery : IQuery<QuestionnaireResponse?>
{
    public Guid AssignmentId { get; }

    public GetResponseByAssignmentIdQuery(Guid assignmentId)
    {
        AssignmentId = assignmentId;
    }
}