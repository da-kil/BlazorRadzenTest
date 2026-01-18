using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Core.Infrastructure;
using ti8m.BeachBreak.Core.Infrastructure.Authorization;

namespace ti8m.BeachBreak.CommandApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for authorization cache management.
/// </summary>
public static class AuthorizationCacheEndpoints
{
    /// <summary>
    /// Maps authorization cache management endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapAuthorizationCacheEndpoints(this WebApplication app)
    {
        var cacheGroup = app.MapGroup("/c/api/v{version:apiVersion}/authorizationcache")
            .WithTags("Authorization Cache")
            .RequireAuthorization("Admin");

        // Invalidate employee authorization cache
        cacheGroup.MapDelete("/{employeeId:guid}", async (
            Guid employeeId,
            IAuthorizationCacheService cacheService,
            [FromServices] ILogger logger,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await cacheService.InvalidateEmployeeRoleCacheAsync(employeeId, cancellationToken);

                logger.LogAuthorizationCacheInvalidated(employeeId, httpContext.User.Identity?.Name ?? "Unknown");

                var result = Result.Success($"Authorization cache invalidated for employee {employeeId}");
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogInvalidateAuthorizationCacheError(ex, employeeId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while invalidating the authorization cache.",
                    statusCode: 500);
            }
        })
        .WithName("InvalidateEmployeeAuthorizationCache")
        .WithSummary("Invalidate authorization cache for a specific employee")
        .WithDescription("Forces the system to reload the employee's role from the database on their next request.")
        .Produces(200)
        .Produces(401)
        .Produces(403)
        .Produces(500);
    }
}