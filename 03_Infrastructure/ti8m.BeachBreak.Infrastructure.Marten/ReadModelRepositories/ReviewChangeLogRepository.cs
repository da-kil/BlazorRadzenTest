using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

/// <summary>
/// Marten implementation of IReviewChangeLogRepository.
/// Provides access to ReviewChangeLog read models.
/// </summary>
internal class ReviewChangeLogRepository(IDocumentStore store) : IReviewChangeLogRepository
{
    public async Task<List<ReviewChangeLogReadModel>> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        var changes = await session.Query<ReviewChangeLogReadModel>()
            .Where(c => c.AssignmentId == assignmentId)
            .OrderBy(c => c.ChangedAt)
            .ToListAsync(cancellationToken);

        return changes.ToList();
    }
}