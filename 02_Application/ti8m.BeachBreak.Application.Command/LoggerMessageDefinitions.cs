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
}