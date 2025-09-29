using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface IOrganizationApiService
{
    Task<List<Organization>> GetAllOrganizationsAsync(bool includeDeleted = false, bool includeIgnored = false);
    Task<Organization?> GetOrganizationByIdAsync(Guid id);
    Task<Organization?> GetOrganizationByNumberAsync(string number);
    Task<List<Organization>> GetOrganizationsByParentIdAsync(Guid parentId);
}
