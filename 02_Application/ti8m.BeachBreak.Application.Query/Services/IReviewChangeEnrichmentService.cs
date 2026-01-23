namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Service for enriching review change data with employee information.
/// Provides batch fetching of employee names to avoid N+1 query problems.
/// </summary>
public interface IReviewChangeEnrichmentService
{
    /// <summary>
    /// Batch fetches employee names for the given employee IDs.
    /// Returns a dictionary mapping employee ID to full name.
    /// </summary>
    /// <param name="employeeIds">Collection of employee IDs to fetch names for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary mapping employee ID to full name (FirstName LastName)</returns>
    Task<Dictionary<Guid, string>> GetEmployeeNamesAsync(
        IEnumerable<Guid> employeeIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the full name for a single employee.
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Full name (FirstName LastName) or "Unknown" if not found</returns>
    Task<string> GetEmployeeNameAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);
}
