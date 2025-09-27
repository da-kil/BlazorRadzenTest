using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Repositories;

public interface IOrganizationRepository : IRepository
{
    Task<OrganizationReadModel?> GetOrganizationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrganizationReadModel>> GetAllOrganizationsAsync(bool includeDeleted = false, bool includeIgnored = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrganizationReadModel>> GetOrganizationsByParentIdAsync(Guid? parentId, bool includeDeleted = false, bool includeIgnored = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrganizationReadModel>> GetOrganizationsByManagerIdAsync(string managerId, bool includeDeleted = false, bool includeIgnored = false, CancellationToken cancellationToken = default);
}