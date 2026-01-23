using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.QueryApi.Authorization;

namespace ti8m.BeachBreak.QueryApi.Controllers;

public class BaseController : ControllerBase
{
    protected IActionResult CreateResponse<TPayload, TMappedPayload>(Result<TPayload> result, Func<TPayload, TMappedPayload> map)
    {
        return result.Succeeded ? Ok(map(result.Payload!)) : Problem(detail: result.Message, statusCode: result.StatusCode);
    }

    protected IActionResult CreateResponse<TPayload>(Result<TPayload> result)
    {
        return result.Succeeded ? Ok(result.Payload) : Problem(detail: result.Message, statusCode: result.StatusCode);
    }

    protected IActionResult CreateResponse(Result result)
    {
        return result.Succeeded ? Ok(string.IsNullOrWhiteSpace(result.Message) ? null : new { result.Message }) : Problem(detail: result.Message, statusCode: result.StatusCode);
    }

    /// <summary>
    /// Executes a query action with manager authorization checks.
    /// Checks if the manager has elevated role (HR/HRLead/Admin) or resource-specific access.
    /// </summary>
    /// <typeparam name="TResult">The query result type</typeparam>
    /// <param name="authorizationService">The manager authorization service</param>
    /// <param name="employeeRoleService">The employee role service for role checks</param>
    /// <param name="logger">Logger for authorization failures</param>
    /// <param name="action">The action to execute with authorized manager ID and elevated role flag</param>
    /// <param name="resourceId">Optional resource ID to check access (e.g., assignmentId, employeeId)</param>
    /// <param name="requiresResourceAccess">Whether to check resource-specific access</param>
    /// <param name="resourceAccessCheck">Custom resource access check function (e.g., IsManagerOfAsync)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>IActionResult with appropriate HTTP response</returns>
    protected async Task<IActionResult> ExecuteWithAuthorizationAsync<TResult>(
        IManagerAuthorizationService authorizationService,
        IEmployeeRoleService employeeRoleService,
        ILogger logger,
        Func<Guid, bool, Task<Result<TResult>>> action,
        Guid? resourceId = null,
        bool requiresResourceAccess = true,
        Func<Guid, Guid, Task<bool>>? resourceAccessCheck = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get manager ID
            Guid managerId;
            try
            {
                managerId = await authorizationService.GetCurrentManagerIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("Authorization failed: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }

            // Check elevated role
            var hasElevatedRole = await HasElevatedRoleAsync(employeeRoleService, logger, managerId, cancellationToken);

            // If not elevated and resource access required, check access
            if (!hasElevatedRole && requiresResourceAccess && resourceId.HasValue)
            {
                bool canAccess;

                if (resourceAccessCheck != null)
                {
                    // Use custom access check (e.g., IsManagerOfAsync for employee access)
                    canAccess = await resourceAccessCheck(managerId, resourceId.Value);
                }
                else
                {
                    // Default to assignment access check
                    canAccess = await authorizationService.CanAccessAssignmentAsync(managerId, resourceId.Value);
                }

                if (!canAccess)
                {
                    logger.LogWarning("Manager {ManagerId} attempted unauthorized access to resource {ResourceId}",
                        managerId, resourceId);
                    return Forbid();
                }
            }

            // Execute action
            var result = await action(managerId, hasElevatedRole);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing authorized action");
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Checks if the current user has an elevated role (HR, HRLead, or Admin).
    /// </summary>
    /// <param name="employeeRoleService">Employee role service</param>
    /// <param name="logger">Logger for warnings</param>
    /// <param name="userId">User ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user has elevated role, false otherwise</returns>
    protected static async Task<bool> HasElevatedRoleAsync(
        IEmployeeRoleService employeeRoleService,
        ILogger logger,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var employeeRole = await employeeRoleService.GetEmployeeRoleAsync(userId, cancellationToken);
        if (employeeRole == null)
        {
            logger.LogWarning("Unable to retrieve employee role for user {UserId}", userId);
            return false;
        }

        return employeeRole.ApplicationRole == Application.Query.Models.ApplicationRole.HR ||
               employeeRole.ApplicationRole == Application.Query.Models.ApplicationRole.HRLead ||
               employeeRole.ApplicationRole == Application.Query.Models.ApplicationRole.Admin;
    }
}