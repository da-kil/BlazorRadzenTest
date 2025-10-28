using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service for authentication and authorization operations.
/// Provides access to current user's role and identity information.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Gets the current user's application role and employee ID.
    /// Calls the backend API to retrieve role from database.
    /// </summary>
    Task<UserRole?> GetMyRoleAsync();
}

/// <summary>
/// DTO for user role information
/// </summary>
public class UserRole
{
    public Guid EmployeeId { get; set; }
    public ApplicationRole ApplicationRole { get; set; }
}
