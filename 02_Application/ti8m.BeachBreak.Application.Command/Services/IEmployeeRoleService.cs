using ti8m.BeachBreak.Core.Infrastructure.Authorization;

namespace ti8m.BeachBreak.Application.Command.Services;

/// <summary>
/// Service for retrieving employee roles with cache-through pattern.
/// Follows clean architecture by providing an Application layer abstraction
/// for role retrieval that handles caching transparently.
/// </summary>
public interface IEmployeeRoleService
{
    /// <summary>
    /// Gets employee role with cache-through pattern.
    /// First checks cache, then database if not found, then updates cache.
    /// This ensures reliable access to employee role data regardless of cache state.
    /// </summary>
    /// <param name="userId">The user ID to get role for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Employee role result or null if user not found</returns>
    Task<EmployeeRole?> GetEmployeeRoleAsync(Guid userId, CancellationToken cancellationToken = default);
}