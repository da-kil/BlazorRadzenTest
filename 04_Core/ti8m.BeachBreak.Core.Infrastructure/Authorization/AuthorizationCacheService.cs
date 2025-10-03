using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace ti8m.BeachBreak.Core.Infrastructure.Authorization;

/// <summary>
/// Service for managing authorization-related cache operations.
///
/// Cache Invalidation Strategy:
/// - Employee role cache has a 15-minute TTL (Time To Live)
/// - When an employee's ApplicationRole changes, they will see the new permissions within 15 minutes
/// - For immediate invalidation, call InvalidateEmployeeRoleCacheAsync in command handlers that change roles
/// - The cache is automatically populated on first authorization check after invalidation or expiration
///
/// Example: To implement automatic cache invalidation on EmployeeApplicationRoleChanged events,
/// inject IAuthorizationCacheService into the command handler that calls Employee.ChangeApplicationRole()
/// and call InvalidateEmployeeRoleCacheAsync after saving the aggregate.
/// </summary>
public class AuthorizationCacheService : IAuthorizationCacheService
{
    private readonly IDistributedCache cache;
    private readonly ILogger<AuthorizationCacheService> logger;

    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
    };

    public AuthorizationCacheService(
        IDistributedCache cache,
        ILogger<AuthorizationCacheService> logger)
    {
        this.cache = cache;
        this.logger = logger;
    }

    public async Task<T?> GetEmployeeRoleCacheAsync<T>(Guid userId, CancellationToken cancellationToken = default) where T : class
    {
        var cacheKey = $"employee-role:{userId}";

        try
        {
            var cachedBytes = await cache.GetAsync(cacheKey, cancellationToken);
            if (cachedBytes != null)
            {
                var cachedJson = Encoding.UTF8.GetString(cachedBytes);
                var cached = JsonSerializer.Deserialize<T>(cachedJson);
                if (cached != null)
                {
                    logger.LogDebug("Employee role retrieved from cache for user ID: {UserId}", userId);
                    return cached;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving employee role from cache for user ID: {UserId}", userId);
        }

        return null;
    }

    public async Task SetEmployeeRoleCacheAsync<T>(Guid userId, T data, CancellationToken cancellationToken = default) where T : class
    {
        var cacheKey = $"employee-role:{userId}";

        try
        {
            var json = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json);
            await cache.SetAsync(cacheKey, bytes, CacheOptions, cancellationToken);
            logger.LogDebug("Employee role cached for user ID: {UserId}", userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error caching employee role for user ID: {UserId}", userId);
        }
    }

    public async Task InvalidateEmployeeRoleCacheAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"employee-role:{userId}";

        try
        {
            await cache.RemoveAsync(cacheKey, cancellationToken);
            logger.LogDebug("Invalidated cached role for user ID: {UserId}", userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating employee role cache for user ID: {UserId}", userId);
        }
    }
}
