using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

internal class EmployeeRepository(IDocumentStore store) : IEmployeeRepository
{
    public async Task<IEnumerable<EmployeeReadModel>> GetEmployeesAsync(bool includeDeleted = false, int? organizationNumber = null, string? role = null, Guid? managerId = null, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();

        // Build the query in a single expression to avoid Marten intermediate assignment issues
        var baseQuery = session.Query<EmployeeReadModel>();

        if (!includeDeleted && organizationNumber.HasValue && !string.IsNullOrWhiteSpace(role) && managerId.HasValue)
        {
            var orgNum = organizationNumber.Value;
            var managerIdStr = managerId.Value.ToString();
            return await baseQuery
                .Where(e => !e.IsDeleted && e.OrganizationNumber == orgNum && e.Role == role && e.ManagerId == managerIdStr)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (!includeDeleted && organizationNumber.HasValue && !string.IsNullOrWhiteSpace(role))
        {
            var orgNum = organizationNumber.Value;
            return await baseQuery
                .Where(e => !e.IsDeleted && e.OrganizationNumber == orgNum && e.Role == role)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (!includeDeleted && organizationNumber.HasValue && managerId.HasValue)
        {
            var orgNum = organizationNumber.Value;
            var managerIdStr = managerId.Value.ToString();
            return await baseQuery
                .Where(e => !e.IsDeleted && e.OrganizationNumber == orgNum && e.ManagerId == managerIdStr)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (!includeDeleted && !string.IsNullOrWhiteSpace(role) && managerId.HasValue)
        {
            var managerIdStr = managerId.Value.ToString();
            return await baseQuery
                .Where(e => !e.IsDeleted && e.Role == role && e.ManagerId == managerIdStr)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (!includeDeleted && organizationNumber.HasValue)
        {
            var orgNum = organizationNumber.Value;
            return await baseQuery
                .Where(e => !e.IsDeleted && e.OrganizationNumber == orgNum)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (!includeDeleted && !string.IsNullOrWhiteSpace(role))
        {
            return await baseQuery
                .Where(e => !e.IsDeleted && e.Role == role)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (!includeDeleted && managerId.HasValue)
        {
            var managerIdStr = managerId.Value.ToString();
            return await baseQuery
                .Where(e => !e.IsDeleted && e.ManagerId == managerIdStr)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (!includeDeleted)
        {
            return await baseQuery
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (organizationNumber.HasValue && !string.IsNullOrWhiteSpace(role) && managerId.HasValue)
        {
            var orgNum = organizationNumber.Value;
            var managerIdStr = managerId.Value.ToString();
            return await baseQuery
                .Where(e => e.OrganizationNumber == orgNum && e.Role == role && e.ManagerId == managerIdStr)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (organizationNumber.HasValue && !string.IsNullOrWhiteSpace(role))
        {
            var orgNum = organizationNumber.Value;
            return await baseQuery
                .Where(e => e.OrganizationNumber == orgNum && e.Role == role)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (organizationNumber.HasValue && managerId.HasValue)
        {
            var orgNum = organizationNumber.Value;
            var managerIdStr = managerId.Value.ToString();
            return await baseQuery
                .Where(e => e.OrganizationNumber == orgNum && e.ManagerId == managerIdStr)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(role) && managerId.HasValue)
        {
            var managerIdStr = managerId.Value.ToString();
            return await baseQuery
                .Where(e => e.Role == role && e.ManagerId == managerIdStr)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (organizationNumber.HasValue)
        {
            var orgNum = organizationNumber.Value;
            return await baseQuery
                .Where(e => e.OrganizationNumber == orgNum)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(role))
        {
            return await baseQuery
                .Where(e => e.Role == role)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else if (managerId.HasValue)
        {
            var managerIdStr = managerId.Value.ToString();
            return await baseQuery
                .Where(e => e.ManagerId == managerIdStr)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
        else
        {
            return await baseQuery
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(cancellationToken);
        }
    }

    public async Task<EmployeeReadModel?> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<EmployeeReadModel>()
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<EmployeeReadModel?> GetEmployeeByLoginNameAsync(string loginName, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<EmployeeReadModel>()
            .Where(e => e.LoginName == loginName && !e.IsDeleted)
            .SingleOrDefaultAsync(cancellationToken);
    }
}