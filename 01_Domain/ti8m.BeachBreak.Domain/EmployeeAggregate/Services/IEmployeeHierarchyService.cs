namespace ti8m.BeachBreak.Domain.EmployeeAggregate.Services;

/// <summary>
/// Domain service for employee hierarchy operations.
/// Handles team membership checks, manager relationships, and hierarchical queries.
/// </summary>
public interface IEmployeeHierarchyService
{
    /// <summary>
    /// Checks if an employee is in a TeamLead's team hierarchy (direct or indirect report).
    /// </summary>
    /// <param name="teamLeadId">The TeamLead's employee ID</param>
    /// <param name="employeeId">The employee ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the employee is in the TeamLead's hierarchy, false otherwise</returns>
    Task<bool> IsInTeamHierarchyAsync(Guid teamLeadId, Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a manager is a direct manager of an employee (1 level only).
    /// </summary>
    /// <param name="managerId">The manager's employee ID</param>
    /// <param name="employeeId">The employee ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the manager is the direct manager, false otherwise</returns>
    Task<bool> IsDirectManagerOfAsync(Guid managerId, Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all employee IDs in a TeamLead's team hierarchy (direct and indirect reports).
    /// </summary>
    /// <param name="teamLeadId">The TeamLead's employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of employee IDs in the hierarchy</returns>
    Task<List<Guid>> GetTeamHierarchyIdsAsync(Guid teamLeadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all direct report IDs for a manager (1 level only).
    /// </summary>
    /// <param name="managerId">The manager's employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of direct report employee IDs</returns>
    Task<List<Guid>> GetDirectReportIdsAsync(Guid managerId, CancellationToken cancellationToken = default);
}
