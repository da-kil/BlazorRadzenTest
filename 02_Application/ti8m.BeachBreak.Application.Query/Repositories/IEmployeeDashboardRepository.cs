using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Repositories;

public interface IEmployeeDashboardRepository : IRepository
{
    Task<EmployeeDashboardReadModel?> GetDashboardByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
}
