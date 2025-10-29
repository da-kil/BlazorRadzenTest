using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Core.Infrastructure;
using ti8m.BeachBreak.Core.Infrastructure.Authorization;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{v:apiVersion}/authorizationcache")]
[Authorize(Policy = "Admin")]
public class AuthorizationCacheController : BaseController
{
    private readonly IAuthorizationCacheService cacheService;
    private readonly ILogger<AuthorizationCacheController> logger;

    public AuthorizationCacheController(
        IAuthorizationCacheService cacheService,
        ILogger<AuthorizationCacheController> logger)
    {
        this.cacheService = cacheService;
        this.logger = logger;
    }

    /// <summary>
    /// Invalidates the authorization cache for a specific employee.
    /// This forces the system to reload the employee's role from the database on their next request.
    /// </summary>
    /// <param name="employeeId">The ID of the employee whose cache should be invalidated</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    [HttpDelete("{employeeId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> InvalidateEmployeeCache(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        await cacheService.InvalidateEmployeeRoleCacheAsync(employeeId, cancellationToken);

        logger.LogAuthorizationCacheInvalidated(employeeId, User.Identity?.Name ?? "Unknown");

        return CreateResponse(Result.Success($"Authorization cache invalidated for employee {employeeId}"));
    }
}
