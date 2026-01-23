namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Service for enriching data with employee names from user IDs.
/// Used in event sourcing architecture to derive display names from immutable user ID facts.
/// </summary>
public interface IEmployeeNameEnrichmentService
{
    /// <summary>
    /// Gets the full name for a single employee by their user ID.
    /// Returns "Unknown" if the employee is not found.
    /// </summary>
    /// <param name="employeeId">The employee's user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Full name in "FirstName LastName" format, or "Unknown"</returns>
    Task<string> GetEmployeeNameAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch fetches employee names for multiple user IDs in a single query.
    /// More efficient than calling GetEmployeeNameAsync multiple times.
    /// </summary>
    /// <param name="employeeIds">Collection of employee user IDs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary mapping employee ID to full name</returns>
    Task<Dictionary<Guid, string>> GetEmployeeNamesAsync(IEnumerable<Guid> employeeIds, CancellationToken cancellationToken = default);
}
