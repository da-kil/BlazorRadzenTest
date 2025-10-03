using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Core.Infrastructure.Authorization;

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

    // Policy to ApplicationRole mappings
    // All roles inherit Employee's basic access and can access additional policy-protected endpoints as defined below
    private static readonly Dictionary<string, ApplicationRole[]> PolicyRoleMappings = new()
    {
        ["EmployeeAccess"] = [ApplicationRole.Employee, ApplicationRole.TeamLead, ApplicationRole.HR, ApplicationRole.HRLead, ApplicationRole.Admin],
        ["AdminOnly"] = [ApplicationRole.Admin],
        ["HRAccess"] = [ApplicationRole.HR, ApplicationRole.HRLead, ApplicationRole.Admin],
        ["HRLeadOnly"] = [ApplicationRole.HRLead, ApplicationRole.Admin],
        ["TeamLeadOrHigher"] = [ApplicationRole.TeamLead, ApplicationRole.HR, ApplicationRole.HRLead, ApplicationRole.Admin],
        ["ManagerAccess"] = [ApplicationRole.TeamLead, ApplicationRole.HR, ApplicationRole.HRLead, ApplicationRole.Admin]
    };

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

        // Check if user has required role for the policy
        var requiredPolicies = policy.Requirements
            .OfType<Microsoft.AspNetCore.Authorization.Infrastructure.RolesAuthorizationRequirement>()
            .SelectMany(r => r.AllowedRoles)
            .ToList();

        if (requiredPolicies.Count == 0)
        {
            // No specific role requirements - any authenticated employee can access
            // This includes ApplicationRole.Employee (basic access level)
            logger.LogAuthorizationSucceeded(userId.Value, employeeRole.ApplicationRole.ToString(), context.Request.Path);
            await next(context);
            return;
        }

        // Check each policy to see if user's role satisfies it
        var hasAccess = false;
        foreach (var policyName in requiredPolicies)
        {
            if (PolicyRoleMappings.TryGetValue(policyName, out var allowedRoles))
            {
                if (allowedRoles.Contains(employeeRole.ApplicationRole))
                {
                    hasAccess = true;
                    break;
                }
            }
            else
            {
                // Policy not in our mappings, check if role name matches directly
                if (Enum.TryParse<ApplicationRole>(policyName, out var role) && role == employeeRole.ApplicationRole)
                {
                    hasAccess = true;
                    break;
                }
            }
        }

        if (!hasAccess)
        {
            logger.LogAuthorizationFailedInsufficientPermissions(
                userId.Value,
                employeeRole.ApplicationRole.ToString(),
                string.Join(", ", requiredPolicies),
                context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Insufficient permissions",
                requiredPolicies = requiredPolicies
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
                         ?? user.FindFirst("sub")?.Value
                         ?? user.FindFirst("oid")?.Value;

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
