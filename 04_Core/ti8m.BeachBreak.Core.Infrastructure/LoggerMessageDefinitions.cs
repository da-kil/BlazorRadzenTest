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

    // InReview note operations logging (7001-7010)
    [LoggerMessage(
    EventId = 7001,
    Level = LogLevel.Information,
    Message = "InReview note added for assignment {AssignmentId} by employee {EmployeeId} with note ID {NoteId}")]
    public static partial void InReviewNoteAdded(this ILogger logger, Guid assignmentId, Guid noteId, Guid employeeId, Exception? exception);

    [LoggerMessage(
    EventId = 7002,
    Level = LogLevel.Information,
    Message = "InReview note {NoteId} updated for assignment {AssignmentId} by employee {EmployeeId}")]
    public static partial void InReviewNoteUpdated(this ILogger logger, Guid noteId, Guid assignmentId, Guid employeeId, Exception? exception);

    [LoggerMessage(
    EventId = 7003,
    Level = LogLevel.Information,
    Message = "InReview note {NoteId} deleted for assignment {AssignmentId} by employee {EmployeeId}")]
    public static partial void InReviewNoteDeleted(this ILogger logger, Guid noteId, Guid assignmentId, Guid employeeId, Exception? exception);

    [LoggerMessage(
    EventId = 7004,
    Level = LogLevel.Error,
    Message = "Failed to add InReview note for assignment {AssignmentId}: {ErrorMessage}")]
    public static partial void AddInReviewNoteFailed(this ILogger logger, Guid assignmentId, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 7005,
    Level = LogLevel.Error,
    Message = "Failed to update InReview note {NoteId} for assignment {AssignmentId}: {ErrorMessage}")]
    public static partial void UpdateInReviewNoteFailed(this ILogger logger, Guid noteId, Guid assignmentId, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 7006,
    Level = LogLevel.Error,
    Message = "Failed to delete InReview note {NoteId} for assignment {AssignmentId}: {ErrorMessage}")]
    public static partial void DeleteInReviewNoteFailed(this ILogger logger, Guid noteId, Guid assignmentId, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 7007,
    Level = LogLevel.Warning,
    Message = "Attempted to add InReview note for assignment {AssignmentId} in invalid state {WorkflowState}")]
    public static partial void InReviewNoteInvalidState(this ILogger logger, Guid assignmentId, string workflowState);

    [LoggerMessage(
    EventId = 7008,
    Level = LogLevel.Warning,
    Message = "User {UserId} attempted to modify InReview note {NoteId} without ownership permissions")]
    public static partial void InReviewNoteUnauthorizedModification(this ILogger logger, Guid userId, Guid noteId);
}