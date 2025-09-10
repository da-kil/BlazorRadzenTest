namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class QuestionnaireAssignmentQuery : IQuery<Result<QuestionnaireAssignment>>
{
    public Guid Id { get; init; }

    public QuestionnaireAssignmentQuery(Guid id)
    {
        Id = id;
    }
}
