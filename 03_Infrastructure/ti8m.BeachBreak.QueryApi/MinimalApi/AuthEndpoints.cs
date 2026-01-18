using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Core.Infrastructure;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.QueryApi.Mappers;

namespace ti8m.BeachBreak.QueryApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for authentication and user role queries.
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Maps authentication query endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var authGroup = app.MapGroup("/q/api/v{version:apiVersion}/auth")
            .WithTags("Auth")
            .RequireAuthorization(); // Only requires authentication, no role check

        // Get current user's role
        authGroup.MapGet("/me/role", async (
            IQueryDispatcher queryDispatcher,
            [FromServices] ILogger logger,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default) =>
        {
            var userId = GetUserId(user);
            if (userId == null)
            {
                logger.LogUserIdNotFoundInClaims();
                return Results.Problem(
                    detail: "User ID not found in claims",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            try
            {
                var result = await queryDispatcher.QueryAsync(
                    new GetEmployeeRoleByIdQuery(userId.Value),
                    cancellationToken);

                if (result == null)
                {
                    logger.LogEmployeeNotFoundForUserId(userId.Value);
                    return Results.Problem(
                        detail: "Employee not found",
                        statusCode: StatusCodes.Status404NotFound);
                }

                return Results.Ok(new UserRoleDto
                {
                    EmployeeId = result.EmployeeId,
                    ApplicationRole = ApplicationRoleMapper.MapToDomain(result.ApplicationRole)
                });
            }
            catch (Exception ex)
            {
                logger.LogGetApplicationRoleError(ex, userId.Value);
                return Results.Problem(
                    detail: "Failed to retrieve user role",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetMyRole")
        .WithSummary("Get current user's role")
        .WithDescription("Gets the current user's ApplicationRole - bypasses role-based authorization to avoid circular dependency")
        .Produces<UserRoleDto>(200)
        .Produces(401)
        .Produces(404)
        .Produces(500);
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        // Try various claim types for user ID (Entra ID object ID)
        var userIdClaim = user.FindFirst("oid")?.Value
                         ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                         ?? user.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public class UserRoleDto
{
    public Guid EmployeeId { get; set; }
    public ApplicationRole ApplicationRole { get; set; }
}