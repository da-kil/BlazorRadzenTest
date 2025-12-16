using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.FeedbackTemplateQueries;

/// <summary>
/// Handler for GetAllFeedbackTemplatesQuery.
/// Returns all non-deleted feedback templates from the read model.
/// </summary>
public class GetAllFeedbackTemplatesQueryHandler : IQueryHandler<GetAllFeedbackTemplatesQuery, List<FeedbackTemplateReadModel>>
{
    private readonly IFeedbackTemplateRepository repository;

    public GetAllFeedbackTemplatesQueryHandler(IFeedbackTemplateRepository repository)
    {
        this.repository = repository;
    }

    public async Task<List<FeedbackTemplateReadModel>> HandleAsync(GetAllFeedbackTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        var templates = await repository.GetAllAsync(cancellationToken);
        return templates.ToList();
    }
}
