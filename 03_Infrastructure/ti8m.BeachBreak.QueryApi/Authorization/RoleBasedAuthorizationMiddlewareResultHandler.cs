using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using System.Security.Claims;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Core.Infrastructure;
using ti8m.BeachBreak.Core.Infrastructure.Authorization;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.QueryApi.Mappers;

namespace ti8m.BeachBreak.QueryApi.Authorization;

/// <summary>
/// Authorization middleware result handler that validates user roles against controller policies.
/// Uses distributed cache to minimize database queries.
/// </summary>
public class RoleBasedAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly IAuthorizationMiddlewareResultHandler defaultHandler = new AuthorizationMiddlewareResultHandler();
    private readonly IAuthorizationCacheService cacheService;
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<RoleBasedAuthorizationMiddlewareResultHandler> logger;

    // Policy to ApplicationRole mappings - from domain service
    private static Dictionary<string, ApplicationRole[]> PolicyRoleMappings => ApplicationRoleAuthorizationService.PolicyRoleMappings;

    public RoleBasedAuthorizationMiddlewareResultHandler(
        IAuthorizationCacheService cacheService,
        IQueryDispatcher queryDispatcher,
        ILogger<RoleBasedAuthorizationMiddlewareResultHandler> logger)
    {
        this.cacheService = cacheService;
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        // Allow /q/api/v1/auth/me/role endpoint to bypass role checking (for initial role fetch)
        if (context.Request.Path.StartsWithSegments("/q/api/v1/auth/me/role", StringComparison.OrdinalIgnoreCase))
        {
            // Still require authentication, but skip role check
            if (context.User.Identity?.IsAuthenticated ?? false)
            {
                await next(context);
                return;
            }
        }

        // If authorization succeeded without our intervention, continue
        if (authorizeResult.Succeeded)
        {
            await next(context);
            return;
        }

        // If user is not authenticated, let default handler handle it (401)
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        // Get user ID from claims (Entra ID object ID)
        var userId = GetUserId(context.User);
        if (userId == null)
        {
            logger.LogAuthorizationFailedNoUserId();
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "User ID not found in claims" });
            return;
        }

        // Get employee role from cache or database
        var employeeRole = await GetEmployeeRoleAsync(userId.Value, context.RequestAborted);
        if (employeeRole == null)
        {
            logger.LogAuthorizationFailedEmployeeNotFound(userId.Value);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Employee not found or access denied" });
            return;
        }

        // Extract policy names from endpoint metadata
        var endpoint = context.GetEndpoint();
        var authorizeData = endpoint?.Metadata.GetOrderedMetadata<IAuthorizeData>() ?? Array.Empty<IAuthorizeData>();

        var requiredPolicyNames = authorizeData
            .Where(a => !string.IsNullOrEmpty(a.Policy))
            .Select(a => a.Policy!)
            .Distinct()
            .ToList();

        var requiredRoleNames = authorizeData
            .Where(a => !string.IsNullOrEmpty(a.Roles))
            .SelectMany(a => a.Roles!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct()
            .ToList();

        // If no policies or roles are required, any authenticated employee can access
        if (requiredPolicyNames.Count == 0 && requiredRoleNames.Count == 0)
        {
            logger.LogAuthorizationSucceeded(userId.Value, employeeRole.ApplicationRole.ToString(), context.Request.Path);
            await next(context);
            return;
        }

        // Check each policy to see if user's role satisfies it
        var hasAccess = false;

        var domainRole = ApplicationRoleMapper.MapToDomain(employeeRole.ApplicationRole);

        // Check policy-based authorization
        foreach (var policyName in requiredPolicyNames)
        {
            if (PolicyRoleMappings.TryGetValue(policyName, out var allowedRoles))
            {
                if (allowedRoles.Contains(domainRole))
                {
                    hasAccess = true;
                    break;
                }
            }
        }

        // Check role-based authorization (from [Authorize(Roles = "...")])
        if (!hasAccess)
        {
            foreach (var roleName in requiredRoleNames)
            {
                if (PolicyRoleMappings.TryGetValue(roleName, out var allowedRoles))
                {
                    if (allowedRoles.Contains(domainRole))
                    {
                        hasAccess = true;
                        break;
                    }
                }
                else if (Enum.TryParse<ApplicationRole>(roleName, out var role) && role == domainRole)
                {
                    hasAccess = true;
                    break;
                }
            }
        }

        if (!hasAccess)
        {
            var allRequirements = requiredPolicyNames.Concat(requiredRoleNames).ToList();
            logger.LogAuthorizationFailedInsufficientPermissions(
                userId.Value,
                employeeRole.ApplicationRole.ToString(),
                string.Join(", ", allRequirements),
                context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Insufficient permissions",
                requiredPolicies = requiredPolicyNames,
                requiredRoles = requiredRoleNames
            });
            return;
        }

        // User has access, continue
        logger.LogAuthorizationSucceeded(userId.Value, employeeRole.ApplicationRole.ToString(), context.Request.Path);
        await next(context);
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        // Try various claim types for user ID (Entra ID object ID)
        var userIdClaim = user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                         ?? user.FindFirst("oid")?.Value
                         ?? user.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private async Task<EmployeeRoleResult?> GetEmployeeRoleAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            // Try to get from cache
            var cached = await cacheService.GetEmployeeRoleCacheAsync<EmployeeRoleResult>(userId, cancellationToken);
            if (cached != null)
            {
                return cached;
            }

            // Not in cache, query database
            var result = await queryDispatcher.QueryAsync(new GetEmployeeRoleByIdQuery(userId), cancellationToken);
            if (result == null)
            {
                return null;
            }

            // Store in cache
            await cacheService.SetEmployeeRoleCacheAsync(userId, result, cancellationToken);

            logger.LogDebug("Employee role retrieved from database and cached for user ID: {UserId}", userId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving employee role for user ID: {UserId}", userId);
            return null;
        }
    }

}
