using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

internal class OrganizationRepository(IDocumentStore store) : IOrganizationRepository
{
    public async Task<OrganizationReadModel?> GetOrganizationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<OrganizationReadModel>()
            .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<OrganizationReadModel>> GetAllOrganizationsAsync(bool includeDeleted = false, bool includeIgnored = false, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();

        if (!includeDeleted && !includeIgnored)
        {
            return await session.Query<OrganizationReadModel>()
                .Where(o => !o.IsDeleted && !o.IsIgnored)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }
        else if (!includeDeleted)
        {
            return await session.Query<OrganizationReadModel>()
                .Where(o => !o.IsDeleted)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }
        else if (!includeIgnored)
        {
            return await session.Query<OrganizationReadModel>()
                .Where(o => !o.IsIgnored)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }
        else
        {
            return await session.Query<OrganizationReadModel>()
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<OrganizationReadModel>> GetOrganizationsByParentIdAsync(Guid? parentId, bool includeDeleted = false, bool includeIgnored = false, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();

        if (!includeDeleted && !includeIgnored)
        {
            return await session.Query<OrganizationReadModel>()
                .Where(o => o.ParentId == parentId && !o.IsDeleted && !o.IsIgnored)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }
        else if (!includeDeleted)
        {
            return await session.Query<OrganizationReadModel>()
                .Where(o => o.ParentId == parentId && !o.IsDeleted)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }
        else if (!includeIgnored)
        {
            return await session.Query<OrganizationReadModel>()
                .Where(o => o.ParentId == parentId && !o.IsIgnored)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }
        else
        {
            return await session.Query<OrganizationReadModel>()
                .Where(o => o.ParentId == parentId)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<OrganizationReadModel>> GetOrganizationsByManagerIdAsync(string managerId, bool includeDeleted = false, bool includeIgnored = false, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();

        if (!includeDeleted && !includeIgnored)
        {
            return await session.Query<OrganizationReadModel>()
                .Where(o => o.ManagerId == managerId && !o.IsDeleted && !o.IsIgnored)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }
        else if (!includeDeleted)
        {
            return await session.Query<OrganizationReadModel>()
                .Where(o => o.ManagerId == managerId && !o.IsDeleted)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }
        else if (!includeIgnored)
        {
            return await session.Query<OrganizationReadModel>()
                .Where(o => o.ManagerId == managerId && !o.IsIgnored)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }
        else
        {
            return await session.Query<OrganizationReadModel>()
                .Where(o => o.ManagerId == managerId)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }
    }
}