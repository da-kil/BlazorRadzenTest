using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Application.Command.Repositories;

public interface IAggregateRepository : IRepository
{
    Task StoreAsync(AggregateRoot aggregateRoot, CancellationToken cancellationToken);

    Task<TAggregate?> LoadAsync<TAggregate>(
        Guid id,
        int? version = null,
        CancellationToken cancellationToken = default)
        where TAggregate : AggregateRoot;

    Task<TAggregate> LoadRequiredAsync<TAggregate>(
        Guid id,
        int? version = null,
        CancellationToken cancellationToken = default)
        where TAggregate : AggregateRoot;
}
