namespace ti8m.BeachBreak.QueryApi.Authorization;

/// <summary>
/// Service for manager-specific authorization checks.
/// Handles verification of manager-employee relationships and access control.
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
    /// Checks if the requesting user can view the specified manager's team.
    /// Returns true if:
    /// - The requesting user IS the manager
    /// - The requesting user has HR/Admin role
    /// </summary>
    bool CanViewTeam(Guid requestingUserId, Guid targetManagerId);

    /// <summary>
    /// Checks if the specified manager has the employee as a direct report.
    /// </summary>
    Task<bool> IsManagerOfAsync(Guid managerId, Guid employeeId);

    /// <summary>
    /// Gets all direct report IDs for the specified manager.
    /// </summary>
    Task<List<Guid>> GetDirectReportIdsAsync(Guid managerId);

    /// <summary>
    /// Checks if the manager can access the specified assignment
    /// (i.e., the assignment belongs to one of their direct reports).
    /// </summary>
    Task<bool> CanAccessAssignmentAsync(Guid managerId, Guid assignmentId);
}
