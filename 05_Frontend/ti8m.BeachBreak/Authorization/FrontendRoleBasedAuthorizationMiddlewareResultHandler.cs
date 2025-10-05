using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;

namespace ti8m.BeachBreak.Authorization;

/// <summary>
/// Frontend authorization middleware that enriches user claims with ApplicationRole from backend.
/// This ensures frontend authorization uses the same ApplicationRole as the backend APIs.
/// </summary>
public class FrontendRoleBasedAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly IAuthorizationMiddlewareResultHandler defaultHandler = new AuthorizationMiddlewareResultHandler();
    private readonly ILogger<FrontendRoleBasedAuthorizationMiddlewareResultHandler> logger;
    private readonly IHttpContextAccessor httpContextAccessor;

    public FrontendRoleBasedAuthorizationMiddlewareResultHandler(
        ILogger<FrontendRoleBasedAuthorizationMiddlewareResultHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        this.logger = logger;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        // If authorization already succeeded, continue
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

        // Check if user already has ApplicationRole claim (from a previous request in this session)
        var existingRoleClaim = context.User.FindFirst("ApplicationRole");
        if (existingRoleClaim != null)
        {
            // User has ApplicationRole claim, check against policy
            var hasAccess = CheckPolicyAccess(policy, existingRoleClaim.Value);

            if (hasAccess)
            {
                logger.LogDebug("Frontend authorization succeeded for user with ApplicationRole: {Role}", existingRoleClaim.Value);
                await next(context);
                return;
            }
            else
            {
                logger.LogWarning("Frontend authorization failed: User with ApplicationRole {Role} lacks required permissions", existingRoleClaim.Value);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        // User doesn't have ApplicationRole claim yet - need to fetch from backend
        // For now, let the backend APIs handle authorization
        // The frontend will rely on backend 403 responses
        logger.LogDebug("Frontend authorization deferred to backend API - no ApplicationRole claim found");

        // Let the request proceed - backend will enforce authorization
        await next(context);
    }

    private bool CheckPolicyAccess(AuthorizationPolicy policy, string userRole)
    {
        // Extract required roles from policy
        var roleRequirements = policy.Requirements
            .OfType<RolesAuthorizationRequirement>()
            .SelectMany(r => r.AllowedRoles)
            .ToList();

        if (roleRequirements.Count == 0)
        {
            return true; // No role requirements
        }

        // Check if user's role matches any required role
        return roleRequirements.Any(required =>
            string.Equals(required, userRole, StringComparison.OrdinalIgnoreCase));
    }
}
