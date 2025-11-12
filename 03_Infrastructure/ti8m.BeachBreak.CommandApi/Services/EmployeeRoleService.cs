using ti8m.BeachBreak.Application.Command.Services;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Core.Infrastructure.Authorization;

namespace ti8m.BeachBreak.CommandApi.Services;

/// <summary>
/// Service for retrieving employee roles with cache-through pattern.
/// Implements clean architecture by providing reliable access to employee roles
/// while abstracting caching details from controllers.
/// </summary>
public class EmployeeRoleService : IEmployeeRoleService
{
    private readonly IAuthorizationCacheService cacheService;
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<EmployeeRoleService> logger;

    public EmployeeRoleService(
        IAuthorizationCacheService cacheService,
        IQueryDispatcher queryDispatcher,
        ILogger<EmployeeRoleService> logger)
    {
        this.cacheService = cacheService;
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    public async Task<EmployeeRole?> GetEmployeeRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Phase 1: Try to get from cache first
            var cached = await cacheService.GetEmployeeRoleCacheAsync<EmployeeRole>(userId, cancellationToken);
            if (cached != null)
            {
                logger.LogDebug("Employee role retrieved from cache for user ID: {UserId}", userId);
                return cached;
            }

            logger.LogDebug("Employee role cache miss, querying database for user ID: {UserId}", userId);

            // Phase 2: Cache miss - query database
            var queryResult = await queryDispatcher.QueryAsync(new GetEmployeeRoleByIdQuery(userId), cancellationToken);
            if (queryResult == null)
            {
                logger.LogWarning("Employee role not found in database for user ID: {UserId}. User may not exist or may not have been properly onboarded.", userId);
                return null;
            }

            // Map from Query model to shared model
            var result = new EmployeeRole(queryResult.EmployeeId, (int)queryResult.ApplicationRole);

            // Phase 3: Store in cache for next time (with 15-minute TTL)
            await cacheService.SetEmployeeRoleCacheAsync(userId, result, cancellationToken);

            logger.LogInformation("Employee role retrieved from database and cached for user ID: {UserId}, Role: {Role}",
                userId, result.ApplicationRoleValue);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving employee role for user ID: {UserId}. This will result in authorization failure.", userId);
            return null;
        }
    }
}