using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Models;
using ti8m.BeachBreak.Application.Command.Services;
using ti8m.BeachBreak.CommandApi.Authorization;
using ti8m.BeachBreak.CommandApi.Mappers;

namespace ti8m.BeachBreak.CommandApi.Services;

/// <summary>
/// Service for authorizing manager actions on the command side.
/// Encapsulates authorization business logic in the application layer following Clean Architecture.
/// </summary>
public class CommandAuthorizationService : ICommandAuthorizationService
{
    private readonly IManagerAuthorizationService managerAuthService;
    private readonly IEmployeeRoleService employeeRoleService;
    private readonly ILogger<CommandAuthorizationService> logger;

    public CommandAuthorizationService(
        IManagerAuthorizationService managerAuthService,
        IEmployeeRoleService employeeRoleService,
        ILogger<CommandAuthorizationService> logger)
    {
        this.managerAuthService = managerAuthService;
        this.employeeRoleService = employeeRoleService;
        this.logger = logger;
    }

    public async Task<Result<Guid>> AuthorizeManagerActionAsync(
        Guid? resourceId = null,
        bool requiresResourceAccess = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Step 1: Get and validate manager ID from user context
            Guid managerId;
            try
            {
                managerId = await managerAuthService.GetCurrentManagerIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("Authorization failed: {Message}", ex.Message);
                return Result<Guid>.Fail(ex.Message, StatusCodes.Status401Unauthorized);
            }

            // Step 2: Check if user has elevated role (HR/HRLead/Admin) for bypass
            var hasElevatedRole = await HasElevatedRoleAsync(managerId, cancellationToken);

            // Step 3: If not elevated and resource access is required, validate specific resource access
            if (!hasElevatedRole && requiresResourceAccess && resourceId.HasValue)
            {
                var canAccess = await managerAuthService.CanAccessAssignmentAsync(managerId, resourceId.Value);
                if (!canAccess)
                {
                    logger.LogWarning(
                        "Manager {ManagerId} attempted unauthorized access to resource {ResourceId}",
                        managerId, resourceId);
                    return Result<Guid>.Fail(
                        "Access denied: You do not have permission to access this resource",
                        StatusCodes.Status403Forbidden);
                }
            }

            // Authorization successful - return manager ID
            return Result<Guid>.Success(managerId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during authorization check");
            return Result<Guid>.Fail(
                "An error occurred during authorization",
                StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Checks if a user has elevated role (HR, HRLead, or Admin).
    /// </summary>
    private async Task<bool> HasElevatedRoleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var employeeRole = await employeeRoleService.GetEmployeeRoleAsync(userId, cancellationToken);
        if (employeeRole == null)
        {
            logger.LogWarning("Unable to retrieve employee role for user {UserId}", userId);
            return false;
        }

        var queryRole = (Application.Query.Models.ApplicationRole)employeeRole.ApplicationRoleValue;
        var commandRole = ApplicationRoleMapper.MapFromQuery(queryRole);
        return commandRole == ApplicationRole.HR ||
               commandRole == ApplicationRole.HRLead ||
               commandRole == ApplicationRole.Admin;
    }
}
