namespace ti8m.BeachBreak.CommandApi.Authorization;

/// <summary>
/// Service for manager-specific authorization checks on the command side.
/// Handles verification of manager-employee relationships and access control for write operations.
/// </summary>
public interface IManagerAuthorizationService
{
    /// <summary>
    /// Gets the current authenticated manager's ID from the user context.
    /// </summary>
    /// <returns>The manager's ID</returns>
    /// <exception cref="UnauthorizedAccessException">When user ID cannot be retrieved or parsed</exception>
    Task<Guid> GetCurrentManagerIdAsync();

    /// <summary>
    /// Checks if all specified employees are direct reports of the manager.
    /// </summary>
    /// <param name="managerId">The manager's ID</param>
    /// <param name="employeeIds">List of employee IDs to validate</param>
    /// <returns>True if all employees are direct reports, false otherwise</returns>
    Task<bool> AreAllDirectReportsAsync(Guid managerId, IEnumerable<Guid> employeeIds);

    /// <summary>
    /// Checks if the specified manager has the employee as a direct report.
    /// </summary>
    Task<bool> IsManagerOfAsync(Guid managerId, Guid employeeId);

    /// <summary>
    /// Gets all direct report IDs for the specified manager.
    /// </summary>
    Task<List<Guid>> GetDirectReportIdsAsync(Guid managerId);

    /// <summary>
    /// Checks if the manager can access/modify the specified assignment
    /// (i.e., the assignment belongs to one of their direct reports).
    /// </summary>
    Task<bool> CanAccessAssignmentAsync(Guid managerId, Guid assignmentId);
}
