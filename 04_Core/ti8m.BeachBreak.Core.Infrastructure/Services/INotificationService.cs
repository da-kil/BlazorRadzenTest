namespace ti8m.BeachBreak.Core.Infrastructure.Services;

/// <summary>
/// Service for sending notifications to users (email, in-app, etc.)
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a generic notification to a recipient
    /// </summary>
    /// <param name="recipientEmail">Email address of the recipient</param>
    /// <param name="subject">Subject of the notification</param>
    /// <param name="message">Body of the notification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if notification was sent successfully, false otherwise</returns>
    Task<bool> SendNotificationAsync(
        string recipientEmail,
        string subject,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification when a questionnaire is reopened.
    /// Includes structured information and HTML formatting.
    /// </summary>
    /// <param name="recipientEmail">Email address of the recipient</param>
    /// <param name="recipientName">Name of the recipient</param>
    /// <param name="assignmentId">The assignment ID that was reopened</param>
    /// <param name="fromState">Previous workflow state</param>
    /// <param name="toState">New workflow state after reopening</param>
    /// <param name="reopenReason">Reason provided for reopening</param>
    /// <param name="reopenedByName">Name of the person who reopened</param>
    /// <param name="reopenedByRole">Role of the person who reopened</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if notification was sent successfully, false otherwise</returns>
    Task<bool> SendQuestionnaireReopenedNotificationAsync(
        string recipientEmail,
        string recipientName,
        Guid assignmentId,
        string fromState,
        string toState,
        string reopenReason,
        string reopenedByName,
        string reopenedByRole,
        CancellationToken cancellationToken = default);
}
