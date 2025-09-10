namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;

public class QuestionnaireTemplateQuery : IQuery<Result<QuestionnaireTemplate>>
{
    public Guid Id { get; init; }

    public QuestionnaireTemplateQuery(Guid id)
    {
        Id = id;
    }
}
