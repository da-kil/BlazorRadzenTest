using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Repositories;

public interface IHRDashboardRepository : IRepository
{
    Task<HRDashboardReadModel?> GetHRDashboardAsync(CancellationToken cancellationToken = default);
}
