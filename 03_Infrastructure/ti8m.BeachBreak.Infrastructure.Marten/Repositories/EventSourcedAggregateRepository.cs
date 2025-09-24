using Marten;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Core.Infrastructure;

namespace ti8m.BeachBreak.Infrastructure.Marten.Repositories;

public class EventSourcedAggregateRepository : IAggregateRepository
{
    protected readonly IDocumentStore store;
    protected readonly ILogger<EventSourcedAggregateRepository> logger;

    public EventSourcedAggregateRepository(
        IDocumentStore store,
        ILogger<EventSourcedAggregateRepository> logger)
    {
        this.store = store;
        this.logger = logger;
    }

    public async Task StoreAsync(
        AggregateRoot aggregateRootEntity,
        CancellationToken cancellationToken)
    {
        using var session = store.LightweightSession();

        IEnumerable<IDomainEvent> domainEvents = aggregateRootEntity.UncommittedEvents.ToList();
        session.Events.Append(aggregateRootEntity.Id, aggregateRootEntity.Version, domainEvents);

        logger.LogSaveEventStream(domainEvents.Count(), aggregateRootEntity.Id);

        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(true);

        aggregateRootEntity.ClearUncommittedDomainEvents();
    }

    public async Task<TAggregate?> LoadAsync<TAggregate>(
        Guid id,
        int? version = null,
        CancellationToken cancellationToken = default)
        where TAggregate : AggregateRoot
    {
        using var session = store.LightweightSession();

        logger.LogLoadEventStream(id);

        var aggregate = await session.Events
            .AggregateStreamAsync<TAggregate>(
                id,
                version ?? 0,
                token: cancellationToken)
            .ConfigureAwait(true);

        return aggregate;
    }

    public async Task<TAggregate> LoadRequiredAsync<TAggregate>(
        Guid id,
        int? version = null,
        CancellationToken cancellationToken = default)
        where TAggregate : AggregateRoot
    {
        TAggregate aggregate = await LoadAsync<TAggregate>(id, version, cancellationToken)
            ?? throw new InvalidOperationException($"No aggregate stream found with id {id}");

        return aggregate;
    }
}