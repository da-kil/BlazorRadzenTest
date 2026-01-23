using ti8m.BeachBreak.Application.Command.Commands;

namespace ti8m.BeachBreak.Application.Command.Services;

/// <summary>
/// Service for authorizing manager actions on the command side.
/// Encapsulates authorization business logic including elevated role checks and resource access validation.
/// </summary>
public interface ICommandAuthorizationService
{
    /// <summary>
    /// Authorizes a manager action with optional resource-specific access control.
    /// Performs the following checks:
    /// 1. Retrieves and validates current manager ID from user context
    /// 2. Checks if manager has elevated role (HR/HRLead/Admin) for bypass
    /// 3. If not elevated and resource access required, validates specific resource access
    /// </summary>
    /// <param name="resourceId">Optional resource ID (e.g., assignmentId) to check access</param>
    /// <param name="requiresResourceAccess">Whether to enforce resource-specific access checks</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// Success result containing authorized manager ID if checks pass.
    /// Failure result with appropriate status code (401 Unauthorized, 403 Forbidden) if checks fail.
    /// </returns>
    Task<Result<Guid>> AuthorizeManagerActionAsync(
        Guid? resourceId = null,
        bool requiresResourceAccess = true,
        CancellationToken cancellationToken = default);
}
