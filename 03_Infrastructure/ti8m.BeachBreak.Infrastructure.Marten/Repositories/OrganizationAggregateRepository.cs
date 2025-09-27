using Marten;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.OrganizationAggregate;

namespace ti8m.BeachBreak.Infrastructure.Marten.Repositories;

internal class OrganizationAggregateRepository(IDocumentStore documentStore, ILogger<EventSourcedAggregateRepository> logger)
    : EventSourcedAggregateRepository(documentStore, logger), IOrganizationAggregateRepository
{
    public async Task<IList<Organization>?> FindEntriesToDeleteAsync<TAggregate>(
        Guid[] ids,
        int? version = null,
        CancellationToken cancellationToken = default)
        where TAggregate : AggregateRoot
    {
        using var session = store.LightweightSession();
        var organizationsToDelete = await session.QueryAsync<OrganizationReadModel>("WHERE (d.data ->> 'Id')::uuid != ALL(?);", ids);

        IList<Organization> organizations = [];

        foreach (var organization in organizationsToDelete)
        {
            var organizationAggregate = await LoadAsync<Organization>(organization.Id, version, cancellationToken);
            if (organizationAggregate != null)
            {
                organizations.Add(organizationAggregate);
            }
        }

        return organizations;
    }
}