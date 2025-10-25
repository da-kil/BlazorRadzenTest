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

    public Task<bool> SendQuestionnaireReopenedNotificationAsync(
        string recipientEmail,
        string recipientName,
        Guid assignmentId,
        string fromState,
        string toState,
        string reopenReason,
        string reopenedByName,
        string reopenedByRole,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var emailBody = BuildReopenedEmailBody(
                recipientName,
                assignmentId,
                fromState,
                toState,
                reopenReason,
                reopenedByName,
                reopenedByRole);

            logger.LogInformation(
                "📧 EMAIL NOTIFICATION - Questionnaire Reopened\n" +
                "To: {RecipientEmail} ({RecipientName})\n" +
                "Subject: Questionnaire Reopened - Action Required\n" +
                "Body:\n{EmailBody}",
                recipientEmail,
                recipientName,
                emailBody);

            // TODO: Implement actual email sending via SMTP
            // Example with MailKit:
            // var message = new MimeMessage();
            // message.From.Add(new MailboxAddress("BeachBreak", "noreply@beachbreak.com"));
            // message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            // message.Subject = "Questionnaire Reopened - Action Required";
            // message.Body = new TextPart("html") { Text = emailBody };
            // await _smtpClient.SendAsync(message, cancellationToken);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send questionnaire reopened notification to {RecipientEmail}", recipientEmail);
            return Task.FromResult(false);
        }
    }

    private static string BuildReopenedEmailBody(
        string recipientName,
        Guid assignmentId,
        string fromState,
        string toState,
        string reopenReason,
        string reopenedByName,
        string reopenedByRole)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #0066cc; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
        .reason {{ background-color: #fff; padding: 15px; border-left: 4px solid #ffa500; margin: 15px 0; }}
        .footer {{ color: #666; font-size: 12px; margin-top: 20px; }}
        .button {{ display: inline-block; background-color: #0066cc; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>Questionnaire Reopened - Action Required</h2>
        </div>
        <div class=""content"">
            <p>Hi {recipientName},</p>

            <p>A questionnaire assigned to you has been reopened and requires your attention.</p>

            <h3>Details:</h3>
            <ul>
                <li><strong>Assignment ID:</strong> {assignmentId}</li>
                <li><strong>Previous State:</strong> {fromState}</li>
                <li><strong>Current State:</strong> {toState}</li>
                <li><strong>Reopened By:</strong> {reopenedByName} ({reopenedByRole})</li>
            </ul>

            <div class=""reason"">
                <strong>Reason for Reopening:</strong>
                <p>{reopenReason}</p>
            </div>

            <p>Please log in to the system to review and complete the required changes.</p>

            <p style=""text-align: center; margin: 20px 0;"">
                <a href=""https://beachbreak.com/assignments/{assignmentId}"" class=""button"">View Questionnaire</a>
            </p>

            <div class=""footer"">
                <p>This is an automated message from the BeachBreak system. Please do not reply to this email.</p>
                <p>If you have questions, please contact your HR administrator.</p>
            </div>
        </div>
    </div>
</body>
</html>";
    }
}
