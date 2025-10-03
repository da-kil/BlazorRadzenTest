namespace ti8m.BeachBreak.Core.Infrastructure.Authorization;

/// <summary>
/// Service for managing authorization-related cache operations
/// </summary>
public interface IAuthorizationCacheService
{
    /// <summary>
    /// Gets the cached employee role for a specific user
    /// </summary>
    Task<T?> GetEmployeeRoleCacheAsync<T>(Guid userId, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets the cached employee role for a specific user
    /// </summary>
    Task SetEmployeeRoleCacheAsync<T>(Guid userId, T data, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Invalidates the cached role for a specific user
    /// </summary>
    Task InvalidateEmployeeRoleCacheAsync(Guid userId, CancellationToken cancellationToken = default);
}
