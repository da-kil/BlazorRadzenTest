namespace ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;

public class GetResponseByIdQuery : IQuery<QuestionnaireResponse?>
{
    public Guid Id { get; }

    public GetResponseByIdQuery(Guid id)
    {
        Id = id;
    }
}