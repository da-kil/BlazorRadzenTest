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
}