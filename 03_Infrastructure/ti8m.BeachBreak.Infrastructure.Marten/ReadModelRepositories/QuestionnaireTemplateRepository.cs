using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

internal class QuestionnaireTemplateRepository(IDocumentStore store) : IQuestionnaireTemplateRepository
{
    public async Task<QuestionnaireTemplateReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<QuestionnaireTemplateReadModel>()
            .Where(x => x.Id == id && !x.IsDeleted)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<QuestionnaireTemplateReadModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<QuestionnaireTemplateReadModel>()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<QuestionnaireTemplateReadModel>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<QuestionnaireTemplateReadModel>()
            .Where(x => x.Status == TemplateStatus.Published && !x.IsDeleted)
            .OrderByDescending(x => x.LastPublishedDate)
            .ThenByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<QuestionnaireTemplateReadModel>> GetDraftAsync(CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<QuestionnaireTemplateReadModel>()
            .Where(x => x.Status == TemplateStatus.Draft && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<QuestionnaireTemplateReadModel>> GetArchivedAsync(CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<QuestionnaireTemplateReadModel>()
            .Where(x => x.Status == TemplateStatus.Archived && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<QuestionnaireTemplateReadModel>> GetAssignableAsync(CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<QuestionnaireTemplateReadModel>()
            .Where(x => x.Status == TemplateStatus.Published && !x.IsDeleted)
            .OrderByDescending(x => x.LastPublishedDate)
            .ThenByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }
}