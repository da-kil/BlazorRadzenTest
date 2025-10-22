using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

public class ProjectionReplayRepository : IProjectionReplayRepository
{
    private readonly IDocumentSession session;

    public ProjectionReplayRepository(IDocumentSession session)
    {
        this.session = session;
    }

    public async Task<ProjectionReplayReadModel?> GetByIdAsync(Guid replayId, CancellationToken cancellationToken = default)
    {
        return await session.LoadAsync<ProjectionReplayReadModel>(replayId, cancellationToken);
    }

    public async Task<IEnumerable<ProjectionReplayReadModel>> GetHistoryAsync(int limit, CancellationToken cancellationToken = default)
    {
        return await session.Query<ProjectionReplayReadModel>()
            .OrderByDescending(r => r.StartedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
