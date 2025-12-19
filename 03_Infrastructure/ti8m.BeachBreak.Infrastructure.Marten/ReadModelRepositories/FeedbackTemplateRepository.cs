using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

/// <summary>
/// Marten-based repository for querying FeedbackTemplateReadModel projections.
/// </summary>
internal class FeedbackTemplateRepository(IDocumentStore store) : IFeedbackTemplateRepository
{
    public async Task<FeedbackTemplateReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<FeedbackTemplateReadModel>()
            .Where(x => x.Id == id && !x.IsDeleted)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<FeedbackTemplateReadModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<FeedbackTemplateReadModel>()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FeedbackTemplateReadModel>> GetBySourceTypeAsync(FeedbackSourceType sourceType, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        int sourceTypeInt = (int)sourceType;
        return await session.Query<FeedbackTemplateReadModel>()
            .Where(x => !x.IsDeleted
                && x.AllowedSourceTypes.Contains(sourceTypeInt)
                && x.Status == Domain.QuestionnaireTemplateAggregate.TemplateStatus.Published)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }
}
