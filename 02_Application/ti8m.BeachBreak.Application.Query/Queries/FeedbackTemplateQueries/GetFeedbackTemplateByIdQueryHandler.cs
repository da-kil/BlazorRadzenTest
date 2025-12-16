using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.FeedbackTemplateQueries;

/// <summary>
/// Handler for GetFeedbackTemplateByIdQuery.
/// Returns a single feedback template by ID, or null if not found/deleted.
/// </summary>
public class GetFeedbackTemplateByIdQueryHandler : IQueryHandler<GetFeedbackTemplateByIdQuery, FeedbackTemplateReadModel?>
{
    private readonly IFeedbackTemplateRepository repository;

    public GetFeedbackTemplateByIdQueryHandler(IFeedbackTemplateRepository repository)
    {
        this.repository = repository;
    }

    public async Task<FeedbackTemplateReadModel?> HandleAsync(GetFeedbackTemplateByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await repository.GetByIdAsync(query.TemplateId, cancellationToken);
    }
}
