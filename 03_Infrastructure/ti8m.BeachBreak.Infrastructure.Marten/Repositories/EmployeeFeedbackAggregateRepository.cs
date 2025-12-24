using Marten;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;
using ti8m.BeachBreak.Infrastructure.Marten.Repositories;

namespace ti8m.BeachBreak.Infrastructure.Marten.Repositories;

/// <summary>
/// Marten-based implementation of EmployeeFeedback aggregate repository.
/// Handles event sourcing persistence and aggregate reconstruction.
/// </summary>
public class EmployeeFeedbackAggregateRepository : EventSourcedAggregateRepository, IEmployeeFeedbackAggregateRepository
{
    public EmployeeFeedbackAggregateRepository(IDocumentStore store, ILogger<EventSourcedAggregateRepository> logger)
        : base(store, logger)
    {
    }

    /// <summary>
    /// Loads an EmployeeFeedback aggregate by its ID.
    /// Returns null if not found.
    /// </summary>
    public async Task<EmployeeFeedback?> LoadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await base.LoadAsync<EmployeeFeedback>(id, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Loads an EmployeeFeedback aggregate by its ID.
    /// Throws exception if not found.
    /// </summary>
    public async Task<EmployeeFeedback> LoadRequiredAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await base.LoadRequiredAsync<EmployeeFeedback>(id, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Stores an EmployeeFeedback aggregate.
    /// Handles both new aggregates and updates to existing ones.
    /// </summary>
    public async Task StoreAsync(EmployeeFeedback aggregate, CancellationToken cancellationToken = default)
    {
        await base.StoreAsync(aggregate, cancellationToken);
    }

    /// <summary>
    /// Checks if an EmployeeFeedback aggregate exists.
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var aggregate = await LoadAsync(id, cancellationToken);
        return aggregate != null;
    }
}