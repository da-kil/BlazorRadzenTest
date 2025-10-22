using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Repositories;

public interface IProjectionReplayRepository : IRepository
{
    Task<ProjectionReplayReadModel?> GetByIdAsync(Guid replayId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectionReplayReadModel>> GetHistoryAsync(int limit, CancellationToken cancellationToken = default);
}
