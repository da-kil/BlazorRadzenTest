using Microsoft.Extensions.Logging;

namespace ti8m.BeachBreak.Core.Infrastructure;

public static partial class LoggerMessageDefinitions
{
    [LoggerMessage(
    EventId = 4001,
    Level = LogLevel.Information,
    Message = "Loading events for aggregate `{Id}`.")]
    public static partial void LogLoadEventStream(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 4002,
    Level = LogLevel.Information,
    Message = "Saving events (count: {NumberOfEvents}) for aggregate `{Id}`.")]
    public static partial void LogSaveEventStream(this ILogger logger, long numberOfEvents, Guid id);

    // Authorization logging (4100-4199)
    [LoggerMessage(
    EventId = 4100,
    Level = LogLevel.Warning,
    Message = "Authorization failed: User ID not found in claims")]
    public static partial void LogAuthorizationFailedNoUserId(this ILogger logger);

    [LoggerMessage(
    EventId = 4101,
    Level = LogLevel.Warning,
    Message = "Authorization failed: Employee not found for user ID {UserId}")]
    public static partial void LogAuthorizationFailedEmployeeNotFound(this ILogger logger, Guid userId);

    [LoggerMessage(
    EventId = 4102,
    Level = LogLevel.Warning,
    Message = "Authorization failed: User {UserId} with role {UserRole} lacks required permissions for policies [{RequiredPolicies}] on path {Path}")]
    public static partial void LogAuthorizationFailedInsufficientPermissions(this ILogger logger, Guid userId, string userRole, string requiredPolicies, string path);

    [LoggerMessage(
    EventId = 4103,
    Level = LogLevel.Information,
    Message = "Authorization succeeded: User {UserId} with role {UserRole} accessed path {Path}")]
    public static partial void LogAuthorizationSucceeded(this ILogger logger, Guid userId, string userRole, string path);

    [LoggerMessage(
    EventId = 4104,
    Level = LogLevel.Information,
    Message = "Authorization cache manually invalidated for employee {EmployeeId} by admin {AdminId}")]
    public static partial void LogAuthorizationCacheInvalidated(this ILogger logger, Guid employeeId, string adminId);
}