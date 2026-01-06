using Microsoft.Extensions.Logging;

namespace ti8m.BeachBreak.Application.Command;

public static partial class LoggerMessageDefinitions
{
    // Questionnaire Template Command Operations
    [LoggerMessage(
    EventId = 5001,
    Level = LogLevel.Information,
    Message = "Creating questionnaire template with ID `{Id}` and name `{Name}`.")]
    public static partial void LogCreateQuestionnaireTemplate(this ILogger logger, Guid id, string name);

    [LoggerMessage(
    EventId = 5002,
    Level = LogLevel.Information,
    Message = "Successfully created questionnaire template with ID `{Id}`.")]
    public static partial void LogQuestionnaireTemplateCreated(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 5003,
    Level = LogLevel.Information,
    Message = "Updating questionnaire template with ID `{Id}`.")]
    public static partial void LogUpdateQuestionnaireTemplate(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 5004,
    Level = LogLevel.Information,
    Message = "Successfully updated questionnaire template with ID `{Id}`.")]
    public static partial void LogQuestionnaireTemplateUpdated(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 5005,
    Level = LogLevel.Information,
    Message = "Deleting questionnaire template with ID `{Id}`.")]
    public static partial void LogDeleteQuestionnaireTemplate(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 5006,
    Level = LogLevel.Information,
    Message = "Successfully deleted questionnaire template with ID `{Id}`.")]
    public static partial void LogQuestionnaireTemplateDeleted(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 5007,
    Level = LogLevel.Information,
    Message = "Publishing questionnaire template with ID `{Id}` by user `{PublishedBy}`.")]
    public static partial void LogPublishQuestionnaireTemplate(this ILogger logger, Guid id, string publishedBy);

    [LoggerMessage(
    EventId = 5008,
    Level = LogLevel.Information,
    Message = "Successfully published questionnaire template with ID `{Id}`.")]
    public static partial void LogQuestionnaireTemplatePublished(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 5009,
    Level = LogLevel.Information,
    Message = "Unpublishing questionnaire template with ID `{Id}` to draft status.")]
    public static partial void LogUnpublishQuestionnaireTemplate(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 5010,
    Level = LogLevel.Information,
    Message = "Successfully unpublished questionnaire template with ID `{Id}`.")]
    public static partial void LogQuestionnaireTemplateUnpublished(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 5011,
    Level = LogLevel.Information,
    Message = "Archiving questionnaire template with ID `{Id}`.")]
    public static partial void LogArchiveQuestionnaireTemplate(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 5012,
    Level = LogLevel.Information,
    Message = "Successfully archived questionnaire template with ID `{Id}`.")]
    public static partial void LogQuestionnaireTemplateArchived(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 5013,
    Level = LogLevel.Information,
    Message = "Restoring questionnaire template with ID `{Id}` from archive.")]
    public static partial void LogRestoreQuestionnaireTemplate(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 5014,
    Level = LogLevel.Information,
    Message = "Successfully restored questionnaire template with ID `{Id}`.")]
    public static partial void LogQuestionnaireTemplateRestored(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 5015,
    Level = LogLevel.Error,
    Message = "Failed to create questionnaire template with ID `{Id}`: {ErrorMessage}")]
    public static partial void LogCreateQuestionnaireTemplateFailed(this ILogger logger, Guid id, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 5016,
    Level = LogLevel.Error,
    Message = "Failed to update questionnaire template with ID `{Id}`: {ErrorMessage}")]
    public static partial void LogUpdateQuestionnaireTemplateFailed(this ILogger logger, Guid id, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 5017,
    Level = LogLevel.Error,
    Message = "Failed to delete questionnaire template with ID `{Id}`: {ErrorMessage}")]
    public static partial void LogDeleteQuestionnaireTemplateFailed(this ILogger logger, Guid id, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 5018,
    Level = LogLevel.Error,
    Message = "Failed to publish questionnaire template with ID `{Id}`: {ErrorMessage}")]
    public static partial void LogPublishQuestionnaireTemplateFailed(this ILogger logger, Guid id, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 5019,
    Level = LogLevel.Error,
    Message = "Failed to unpublish questionnaire template with ID `{Id}`: {ErrorMessage}")]
    public static partial void LogUnpublishQuestionnaireTemplateFailed(this ILogger logger, Guid id, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 5020,
    Level = LogLevel.Error,
    Message = "Failed to archive questionnaire template with ID `{Id}`: {ErrorMessage}")]
    public static partial void LogArchiveQuestionnaireTemplateFailed(this ILogger logger, Guid id, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 5021,
    Level = LogLevel.Error,
    Message = "Failed to restore questionnaire template with ID `{Id}`: {ErrorMessage}")]
    public static partial void LogRestoreQuestionnaireTemplateFailed(this ILogger logger, Guid id, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 5022,
    Level = LogLevel.Warning,
    Message = "Questionnaire template with ID `{Id}` not found.")]
    public static partial void LogQuestionnaireTemplateNotFound(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 5023,
    Level = LogLevel.Information,
    Message = "Cloning questionnaire template with ID `{TemplateId}`.")]
    public static partial void LogCloneQuestionnaireTemplate(this ILogger logger, Guid templateId);

    [LoggerMessage(
    EventId = 5024,
    Level = LogLevel.Information,
    Message = "Successfully cloned questionnaire template `{SourceTemplateId}` to new template `{NewTemplateId}`.")]
    public static partial void LogQuestionnaireTemplateCloned(this ILogger logger, Guid sourceTemplateId, Guid newTemplateId);

    [LoggerMessage(
    EventId = 5025,
    Level = LogLevel.Error,
    Message = "Failed to clone questionnaire template with ID `{TemplateId}`: {ErrorMessage}")]
    public static partial void LogCloneQuestionnaireTemplateFailed(this ILogger logger, Guid templateId, string errorMessage, Exception? exception = null);

    // Questionnaire Assignment Command Operations
    [LoggerMessage(
    EventId = 6001,
    Level = LogLevel.Information,
    Message = "Creating assignment `{AssignmentId}` for employee `{EmployeeId}` with template `{TemplateId}`.")]
    public static partial void LogCreateAssignment(this ILogger logger, Guid assignmentId, Guid employeeId, Guid templateId);

    [LoggerMessage(
    EventId = 6002,
    Level = LogLevel.Information,
    Message = "Successfully created assignment `{AssignmentId}`.")]
    public static partial void LogAssignmentCreated(this ILogger logger, Guid assignmentId);

    [LoggerMessage(
    EventId = 6003,
    Level = LogLevel.Information,
    Message = "Creating `{EmployeeCount}` bulk assignments with template `{TemplateId}`.")]
    public static partial void LogCreateBulkAssignments(this ILogger logger, int employeeCount, Guid templateId);

    [LoggerMessage(
    EventId = 6004,
    Level = LogLevel.Information,
    Message = "Successfully created `{AssignmentCount}` bulk assignments.")]
    public static partial void LogBulkAssignmentsCreated(this ILogger logger, int assignmentCount);

    [LoggerMessage(
    EventId = 6005,
    Level = LogLevel.Information,
    Message = "Starting work on assignment `{AssignmentId}`.")]
    public static partial void LogStartAssignmentWork(this ILogger logger, Guid assignmentId);

    [LoggerMessage(
    EventId = 6006,
    Level = LogLevel.Information,
    Message = "Successfully started work on assignment `{AssignmentId}`.")]
    public static partial void LogAssignmentWorkStarted(this ILogger logger, Guid assignmentId);

    [LoggerMessage(
    EventId = 6007,
    Level = LogLevel.Information,
    Message = "Completing work on assignment `{AssignmentId}`.")]
    public static partial void LogCompleteAssignmentWork(this ILogger logger, Guid assignmentId);

    [LoggerMessage(
    EventId = 6008,
    Level = LogLevel.Information,
    Message = "Successfully completed work on assignment `{AssignmentId}`.")]
    public static partial void LogAssignmentWorkCompleted(this ILogger logger, Guid assignmentId);

    [LoggerMessage(
    EventId = 6009,
    Level = LogLevel.Information,
    Message = "Extending due date for assignment `{AssignmentId}` to `{NewDueDate}`.")]
    public static partial void LogExtendAssignmentDueDate(this ILogger logger, Guid assignmentId, DateTime newDueDate);

    [LoggerMessage(
    EventId = 6010,
    Level = LogLevel.Information,
    Message = "Successfully extended due date for assignment `{AssignmentId}`.")]
    public static partial void LogAssignmentDueDateExtended(this ILogger logger, Guid assignmentId);

    [LoggerMessage(
    EventId = 6011,
    Level = LogLevel.Information,
    Message = "Withdrawing assignment `{AssignmentId}`.")]
    public static partial void LogWithdrawAssignment(this ILogger logger, Guid assignmentId);

    [LoggerMessage(
    EventId = 6012,
    Level = LogLevel.Information,
    Message = "Successfully withdrew assignment `{AssignmentId}`.")]
    public static partial void LogAssignmentWithdrawn(this ILogger logger, Guid assignmentId);

    [LoggerMessage(
    EventId = 6013,
    Level = LogLevel.Error,
    Message = "Failed to create questionnaire assignment: {ErrorMessage}")]
    public static partial void LogCreateAssignmentFailed(this ILogger logger, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 6014,
    Level = LogLevel.Error,
    Message = "Failed to create bulk assignments: {ErrorMessage}")]
    public static partial void LogCreateBulkAssignmentsFailed(this ILogger logger, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 6015,
    Level = LogLevel.Error,
    Message = "Failed to start work on assignment `{AssignmentId}`: {ErrorMessage}")]
    public static partial void LogStartAssignmentWorkFailed(this ILogger logger, Guid assignmentId, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 6016,
    Level = LogLevel.Error,
    Message = "Failed to complete work on assignment `{AssignmentId}`: {ErrorMessage}")]
    public static partial void LogCompleteAssignmentWorkFailed(this ILogger logger, Guid assignmentId, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 6017,
    Level = LogLevel.Error,
    Message = "Failed to extend due date for assignment `{AssignmentId}`: {ErrorMessage}")]
    public static partial void LogExtendAssignmentDueDateFailed(this ILogger logger, Guid assignmentId, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 6018,
    Level = LogLevel.Error,
    Message = "Failed to withdraw assignment `{AssignmentId}`: {ErrorMessage}")]
    public static partial void LogWithdrawAssignmentFailed(this ILogger logger, Guid assignmentId, string errorMessage, Exception exception);

    // Assignment Initialization Operations (6100-6119)
    [LoggerMessage(
    EventId = 6100,
    Level = LogLevel.Information,
    Message = "Initializing assignment `{AssignmentId}` by employee `{EmployeeId}`.")]
    public static partial void LogInitializeAssignment(this ILogger logger, Guid assignmentId, Guid employeeId);

    [LoggerMessage(
    EventId = 6101,
    Level = LogLevel.Information,
    Message = "Successfully initialized assignment `{AssignmentId}` by employee `{EmployeeId}`.")]
    public static partial void LogAssignmentInitialized(this ILogger logger, Guid assignmentId, Guid employeeId);

    [LoggerMessage(
    EventId = 6102,
    Level = LogLevel.Error,
    Message = "Failed to initialize assignment `{AssignmentId}`: {ErrorMessage}")]
    public static partial void LogInitializeAssignmentFailed(this ILogger logger, Guid assignmentId, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 6103,
    Level = LogLevel.Information,
    Message = "Adding custom sections to assignment `{AssignmentId}` by employee `{EmployeeId}`.")]
    public static partial void LogAddCustomSections(this ILogger logger, Guid assignmentId, Guid employeeId);

    [LoggerMessage(
    EventId = 6104,
    Level = LogLevel.Information,
    Message = "Successfully added `{SectionCount}` custom sections to assignment `{AssignmentId}`.")]
    public static partial void LogCustomSectionsAdded(this ILogger logger, Guid assignmentId, int sectionCount);

    [LoggerMessage(
    EventId = 6105,
    Level = LogLevel.Error,
    Message = "Failed to add custom sections to assignment `{AssignmentId}`: {ErrorMessage}")]
    public static partial void LogAddCustomSectionsFailed(this ILogger logger, Guid assignmentId, string errorMessage, Exception exception);

    // Employee Role Management Operations (7001-7099)
    [LoggerMessage(
    EventId = 7001,
    Level = LogLevel.Information,
    Message = "Changing application role for employee `{EmployeeId}` to `{NewRole}`.")]
    public static partial void LogChangeEmployeeApplicationRole(this ILogger logger, Guid employeeId, string newRole);

    [LoggerMessage(
    EventId = 7002,
    Level = LogLevel.Information,
    Message = "Successfully changed application role for employee `{EmployeeId}` to `{NewRole}`.")]
    public static partial void LogEmployeeApplicationRoleChanged(this ILogger logger, Guid employeeId, string newRole);

    [LoggerMessage(
    EventId = 7003,
    Level = LogLevel.Error,
    Message = "Failed to change application role for employee `{EmployeeId}`: {ErrorMessage}")]
    public static partial void LogChangeEmployeeApplicationRoleFailed(this ILogger logger, Guid employeeId, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 7004,
    Level = LogLevel.Warning,
    Message = "Attempted to change application role for deleted employee `{EmployeeId}`.")]
    public static partial void LogChangeRoleForDeletedEmployee(this ILogger logger, Guid employeeId);

    // Projection Replay Operations (8001-8099)
    [LoggerMessage(
    EventId = 8001,
    Level = LogLevel.Information,
    Message = "Starting projection replay for `{ProjectionName}` initiated by `{InitiatedBy}`.")]
    public static partial void LogStartProjectionReplay(this ILogger logger, string projectionName, Guid initiatedBy);

    [LoggerMessage(
    EventId = 8002,
    Level = LogLevel.Information,
    Message = "Successfully started projection replay `{ReplayId}` for projection `{ProjectionName}`.")]
    public static partial void LogProjectionReplayStarted(this ILogger logger, Guid replayId, string projectionName);

    [LoggerMessage(
    EventId = 8003,
    Level = LogLevel.Information,
    Message = "Cancelling projection replay `{ReplayId}` by `{CancelledBy}`.")]
    public static partial void LogCancelProjectionReplay(this ILogger logger, Guid replayId, Guid cancelledBy);

    [LoggerMessage(
    EventId = 8004,
    Level = LogLevel.Information,
    Message = "Successfully cancelled projection replay `{ReplayId}`.")]
    public static partial void LogProjectionReplayCancelled(this ILogger logger, Guid replayId);

    [LoggerMessage(
    EventId = 8005,
    Level = LogLevel.Error,
    Message = "Failed to start projection replay for `{ProjectionName}`: {ErrorMessage}")]
    public static partial void LogStartProjectionReplayFailed(this ILogger logger, string projectionName, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 8006,
    Level = LogLevel.Error,
    Message = "Failed to cancel projection replay `{ReplayId}`: {ErrorMessage}")]
    public static partial void LogCancelProjectionReplayFailed(this ILogger logger, Guid replayId, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 8007,
    Level = LogLevel.Warning,
    Message = "Projection `{ProjectionName}` not found or not rebuildable.")]
    public static partial void LogProjectionNotRebuildable(this ILogger logger, string projectionName);

}