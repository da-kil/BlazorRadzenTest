namespace ti8m.BeachBreak.Core.Infrastructure.Services;

/// <summary>
/// Service for sending notifications to users (email, in-app, etc.)
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to a recipient
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
}
