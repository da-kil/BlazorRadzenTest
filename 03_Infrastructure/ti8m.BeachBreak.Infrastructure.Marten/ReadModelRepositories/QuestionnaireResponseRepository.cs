using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

/// <summary>
/// Marten implementation of IQuestionnaireResponseRepository.
/// Provides access to QuestionnaireResponse read models.
/// </summary>
internal class QuestionnaireResponseRepository(IDocumentStore store) : IQuestionnaireResponseRepository
{
    public async Task<QuestionnaireResponseReadModel?> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<QuestionnaireResponseReadModel>()
            .SingleOrDefaultAsync(r => r.AssignmentId == assignmentId, cancellationToken);
    }

    public async Task<List<QuestionnaireResponseReadModel>> GetByAssignmentIdsAsync(IEnumerable<Guid> assignmentIds, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        var results = await session.Query<QuestionnaireResponseReadModel>()
            .Where(r => assignmentIds.Contains(r.AssignmentId))
            .ToListAsync(cancellationToken);
        return results.ToList();
    }

    public async Task<List<QuestionnaireResponseReadModel>> GetAllResponsesAsync(CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        var results = await session.Query<QuestionnaireResponseReadModel>()
            .OrderByDescending(r => r.LastModified)
            .ToListAsync(cancellationToken);
        return results.ToList();
    }

    public async Task<QuestionnaireResponseReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.LoadAsync<QuestionnaireResponseReadModel>(id, cancellationToken);
    }
}
