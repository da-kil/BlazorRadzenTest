using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Implementation of IReviewChangeEnrichmentService.
/// Provides efficient batch fetching of employee names using the employee repository.
/// </summary>
public class ReviewChangeEnrichmentService : IReviewChangeEnrichmentService
{
    private readonly IEmployeeRepository employeeRepository;

    public ReviewChangeEnrichmentService(IEmployeeRepository employeeRepository)
    {
        this.employeeRepository = employeeRepository;
    }

    /// <summary>
    /// Batch fetches employee names for the given employee IDs.
    /// Uses a single repository query and filters in memory for efficiency.
    /// </summary>
    /// <param name="employeeIds">Collection of employee IDs to fetch names for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary mapping employee ID to full name (FirstName LastName)</returns>
    public async Task<Dictionary<Guid, string>> GetEmployeeNamesAsync(
        IEnumerable<Guid> employeeIds,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = employeeIds.Distinct().ToList();

        if (!distinctIds.Any())
            return new Dictionary<Guid, string>();

        // Fetch all employees in a single query
        var allEmployees = await employeeRepository.GetEmployeesAsync(
            includeDeleted: false,
            cancellationToken: cancellationToken);

        // Filter to only the requested IDs and build dictionary
        var employeeDict = allEmployees
            .Where(e => distinctIds.Contains(e.Id))
            .ToDictionary(
                e => e.Id,
                e => $"{e.FirstName} {e.LastName}");

        // Add "Unknown" for any missing employees
        foreach (var employeeId in distinctIds)
        {
            if (!employeeDict.ContainsKey(employeeId))
            {
                employeeDict[employeeId] = "Unknown";
            }
        }

        return employeeDict;
    }

    /// <summary>
    /// Gets the full name for a single employee.
    /// For single lookups, this is less efficient than batch fetching.
    /// Consider using GetEmployeeNamesAsync for multiple employees.
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Full name (FirstName LastName) or "Unknown" if not found</returns>
    public async Task<string> GetEmployeeNameAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetEmployeeByIdAsync(employeeId, cancellationToken);

        if (employee == null)
            return "Unknown";

        return $"{employee.FirstName} {employee.LastName}";
    }
}
