using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Core.Infrastructure.Services;

namespace ti8m.BeachBreak.Infrastructure.Marten.Services;

/// <summary>
/// Console-based notification service for development/testing.
/// In production, replace with actual email service (e.g., SendGrid, SMTP, etc.)
/// </summary>
public class ConsoleNotificationService : INotificationService
{
    private readonly ILogger<ConsoleNotificationService> logger;

    public ConsoleNotificationService(ILogger<ConsoleNotificationService> logger)
    {
        this.logger = logger;
    }

    public Task<bool> SendNotificationAsync(
        string recipientEmail,
        string subject,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Log the notification details
            logger.LogInformation(
                "=== NOTIFICATION SENT ===" + Environment.NewLine +
                "To: {RecipientEmail}" + Environment.NewLine +
                "Subject: {Subject}" + Environment.NewLine +
                "Message: {Message}" + Environment.NewLine +
                "========================",
                recipientEmail, subject, message);

            // In production, this would integrate with:
            // - Email service (SendGrid, AWS SES, SMTP)
            // - In-app notification system
            // - SMS service (Twilio)
            // - Push notifications

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send notification to {RecipientEmail}", recipientEmail);
            return Task.FromResult(false);
        }
    }
}
