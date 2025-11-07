using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Service for retrieving employee roles with cache-through pattern for Query side.
/// Provides reliable access to employee role data regardless of cache state.
/// </summary>
public interface IEmployeeRoleService
{
    /// <summary>
    /// Gets employee role with cache-through pattern.
    /// First checks cache, then database if not found, then updates cache.
    /// </summary>
    /// <param name="userId">The user ID to get role for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Employee role result or null if user not found</returns>
    Task<EmployeeRoleResult?> GetEmployeeRoleAsync(Guid userId, CancellationToken cancellationToken = default);
}