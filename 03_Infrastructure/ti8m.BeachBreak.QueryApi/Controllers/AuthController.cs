using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize] // Only requires authentication, no role check
public class AuthController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<AuthController> logger;

    public AuthController(
        IQueryDispatcher queryDispatcher,
        ILogger<AuthController> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    /// <summary>
    /// Gets the current user's ApplicationRole.
    /// This endpoint bypasses role-based authorization to avoid circular dependency.
    /// </summary>
    [HttpGet("me/role")]
    [ProducesResponseType(typeof(UserRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyRole()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            logger.LogWarning("User ID not found in claims");
            return Problem(
                detail: "User ID not found in claims",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        try
        {
            var result = await queryDispatcher.QueryAsync(
                new GetEmployeeRoleByIdQuery(userId.Value),
                HttpContext.RequestAborted);

            if (result == null)
            {
                logger.LogWarning("Employee not found for user ID: {UserId}", userId);
                return Problem(
                    detail: "Employee not found",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Ok(new UserRoleDto
            {
                EmployeeId = result.EmployeeId,
                ApplicationRole = result.ApplicationRole
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting application role for user {UserId}", userId);
            return Problem(
                detail: "Failed to retrieve user role",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private Guid? GetUserId()
    {
        // Try various claim types for user ID (Entra ID object ID)
        var userIdClaim = User.FindFirst("oid")?.Value
                         ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                         ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public class UserRoleDto
{
    public Guid EmployeeId { get; set; }
    public ApplicationRole ApplicationRole { get; set; }
}
