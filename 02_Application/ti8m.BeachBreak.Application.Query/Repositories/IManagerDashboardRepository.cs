using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Repositories;

public interface IManagerDashboardRepository : IRepository
{
    Task<ManagerDashboardReadModel?> GetDashboardByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default);
}
