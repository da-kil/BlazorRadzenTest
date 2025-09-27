using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.OrganizationAggregate;

namespace ti8m.BeachBreak.Application.Command.Repositories;

public interface IOrganizationAggregateRepository : IAggregateRepository
{
    Task<IList<Organization>?> FindEntriesToDeleteAsync<TAggregate>(
        Guid[] ids,
        int? version = null,
        CancellationToken cancellationToken = default)
        where TAggregate : AggregateRoot;
}