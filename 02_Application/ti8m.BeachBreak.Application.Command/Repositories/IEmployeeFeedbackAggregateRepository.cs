using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Application.Command.Repositories;

/// <summary>
/// Repository interface for EmployeeFeedback aggregate persistence.
/// Follows existing repository patterns in the application.
/// </summary>
public interface IEmployeeFeedbackAggregateRepository : IRepository
{
    /// <summary>
    /// Loads an EmployeeFeedback aggregate by its ID.
    /// Returns null if not found.
    /// </summary>
    /// <param name="id">Aggregate ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>EmployeeFeedback aggregate or null if not found</returns>
    Task<EmployeeFeedback?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an EmployeeFeedback aggregate by its ID.
    /// Throws exception if not found.
    /// </summary>
    /// <param name="id">Aggregate ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>EmployeeFeedback aggregate</returns>
    /// <exception cref="InvalidOperationException">Thrown if aggregate not found</exception>
    Task<EmployeeFeedback> LoadRequiredAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores an EmployeeFeedback aggregate.
    /// Handles both new aggregates and updates to existing ones.
    /// </summary>
    /// <param name="aggregate">Aggregate to store</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StoreAsync(EmployeeFeedback aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an EmployeeFeedback aggregate exists.
    /// </summary>
    /// <param name="id">Aggregate ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if aggregate exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}