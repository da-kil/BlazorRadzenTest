using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Repositories;

public interface IEmployeeRepository : IRepository
{
    Task<IEnumerable<EmployeeReadModel>> GetEmployeesAsync(bool includeDeleted = false, int? organizationNumber = null, string? role = null, Guid? managerId = null, CancellationToken cancellationToken = default);
    Task<EmployeeReadModel?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmployeeReadModel?> GetEmployeeByLoginNameAsync(string loginName, CancellationToken cancellationToken = default);
}