using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.FeedbackTemplateQueries;

/// <summary>
/// Handler for GetFeedbackTemplatesBySourceTypeQuery.
/// Returns templates that allow the specified feedback source type.
/// </summary>
public class GetFeedbackTemplatesBySourceTypeQueryHandler : IQueryHandler<GetFeedbackTemplatesBySourceTypeQuery, List<FeedbackTemplateReadModel>>
{
    private readonly IFeedbackTemplateRepository repository;

    public GetFeedbackTemplatesBySourceTypeQueryHandler(IFeedbackTemplateRepository repository)
    {
        this.repository = repository;
    }

    public async Task<List<FeedbackTemplateReadModel>> HandleAsync(GetFeedbackTemplatesBySourceTypeQuery query, CancellationToken cancellationToken = default)
    {
        var templates = await repository.GetBySourceTypeAsync(query.SourceType, cancellationToken);
        return templates.ToList();
    }
}
