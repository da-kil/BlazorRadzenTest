using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Services;

namespace ti8m.BeachBreak.CommandApi.Controllers;

public class BaseController : ControllerBase
{
    protected IActionResult CreateResponse(Result result)
    {
        return result.Succeeded ? Ok(result) : Problem(detail: result.Message, statusCode: result.StatusCode);
    }

    protected IActionResult CreateResponse<T>(Result<T> result)
    {
        return result.Succeeded ? Ok(result) : Problem(detail: result.Message, statusCode: result.StatusCode);
    }

    /// <summary>
    /// Executes an action with authorization checks for manager access.
    /// Delegates authorization logic to ICommandAuthorizationService following Clean Architecture.
    /// </summary>
    /// <typeparam name="TResult">The result type (Result or Result&lt;T&gt;)</typeparam>
    /// <param name="authorizationService">The command authorization service</param>
    /// <param name="action">The action to execute with authorized manager ID</param>
    /// <param name="resourceId">Optional resource ID to check access (e.g., assignmentId)</param>
    /// <param name="requiresResourceAccess">Whether to check resource-specific access</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>IActionResult with appropriate HTTP response</returns>
    protected async Task<IActionResult> ExecuteWithAuthorizationAsync<TResult>(
        ICommandAuthorizationService authorizationService,
        Func<Guid, Task<TResult>> action,
        Guid? resourceId = null,
        bool requiresResourceAccess = true,
        CancellationToken cancellationToken = default) where TResult : Result
    {
        // Perform authorization checks (business logic in application layer)
        var authResult = await authorizationService.AuthorizeManagerActionAsync(
            resourceId,
            requiresResourceAccess,
            cancellationToken);

        if (!authResult.Succeeded)
        {
            // Return appropriate HTTP status based on authorization failure
            return authResult.StatusCode switch
            {
                StatusCodes.Status401Unauthorized => Unauthorized(authResult.Message),
                StatusCodes.Status403Forbidden => Problem(detail: authResult.Message, statusCode: StatusCodes.Status403Forbidden),
                _ => StatusCode(authResult.StatusCode, authResult.Message)
            };
        }

        // Authorization successful - execute action with authorized manager ID
        var managerId = authResult.Payload!;
        var result = await action(managerId);
        return CreateResponse(result);
    }
}