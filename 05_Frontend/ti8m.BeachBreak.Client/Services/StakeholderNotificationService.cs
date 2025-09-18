using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class StakeholderNotificationService : INotificationService
{
    // In-memory storage for demo purposes - would be database in real implementation
    private readonly List<StakeholderNotification> _notifications = new();
    private readonly Dictionary<Guid, List<string>> _templateStakeholders = new();

    public async Task<bool> SendPublishNotificationAsync(Guid templateId, string templateName, string eventType, List<string> stakeholderEmails)
    {
        try
        {
            var template = await GetNotificationTemplateAsync(eventType);
            var subject = GenerateSubject(eventType, templateName);
            var message = GenerateMessage(template, templateName, eventType);

            foreach (var email in stakeholderEmails.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                var notification = new StakeholderNotification
                {
                    TemplateId = templateId,
                    EventType = eventType,
                    RecipientEmail = email.Trim(),
                    RecipientName = ExtractNameFromEmail(email),
                    RecipientRole = "Stakeholder", // Would determine from user system
                    Subject = subject,
                    Message = message,
                    Status = NotificationStatus.Pending
                };

                _notifications.Add(notification);

                // Simulate sending (would integrate with email service)
                await SimulateSendNotificationAsync(notification);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<StakeholderNotification>> GetNotificationHistoryAsync(Guid templateId)
    {
        await Task.Delay(50);
        return _notifications.Where(n => n.TemplateId == templateId)
                           .OrderByDescending(n => n.SentDate)
                           .ToList();
    }

    public async Task<List<StakeholderNotification>> GetPendingNotificationsAsync()
    {
        await Task.Delay(50);
        return _notifications.Where(n => n.Status == NotificationStatus.Pending)
                           .OrderBy(n => n.SentDate)
                           .ToList();
    }

    public async Task<string> GetNotificationTemplateAsync(string eventType)
    {
        await Task.Delay(50);

        return eventType.ToLower() switch
        {
            "published" => "The questionnaire template '{TemplateName}' has been published and is now available for assignments.",
            "unpublished" => "The questionnaire template '{TemplateName}' has been unpublished and is no longer available for new assignments.",
            "scheduled" => "The questionnaire template '{TemplateName}' has been scheduled for publishing.",
            "scheduledpublish" => "The questionnaire template '{TemplateName}' has been automatically published as scheduled.",
            "approvalrequested" => "A publishing approval request has been submitted for the questionnaire template '{TemplateName}'.",
            "approved" => "The publishing request for questionnaire template '{TemplateName}' has been approved.",
            "rejected" => "The publishing request for questionnaire template '{TemplateName}' has been rejected.",
            _ => "There has been an update to the questionnaire template '{TemplateName}'."
        };
    }

    public async Task<bool> ConfigureStakeholdersAsync(Guid templateId, List<string> stakeholderEmails)
    {
        await Task.Delay(50);

        var validEmails = stakeholderEmails.Where(e => !string.IsNullOrWhiteSpace(e) && IsValidEmail(e))
                                          .Select(e => e.Trim().ToLower())
                                          .Distinct()
                                          .ToList();

        _templateStakeholders[templateId] = validEmails;
        return true;
    }

    public async Task<List<string>> GetStakeholdersAsync(Guid templateId)
    {
        await Task.Delay(50);
        return _templateStakeholders.TryGetValue(templateId, out var stakeholders) ? stakeholders : new List<string>();
    }

    public async Task<bool> ProcessPendingNotificationsAsync()
    {
        var pending = await GetPendingNotificationsAsync();
        var successCount = 0;

        foreach (var notification in pending)
        {
            if (await SimulateSendNotificationAsync(notification))
                successCount++;
        }

        return successCount > 0;
    }

    public async Task<StakeholderNotification?> RetryFailedNotificationAsync(Guid notificationId)
    {
        await Task.Delay(50);

        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification == null || notification.Status != NotificationStatus.Failed)
            return null;

        notification.Status = NotificationStatus.Pending;
        notification.ErrorMessage = string.Empty;

        if (await SimulateSendNotificationAsync(notification))
            return notification;

        return null;
    }

    private async Task<bool> SimulateSendNotificationAsync(StakeholderNotification notification)
    {
        await Task.Delay(100); // Simulate network delay

        // Simulate occasional failures (5% failure rate)
        if (Random.Shared.Next(1, 21) == 1)
        {
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = "Simulated network timeout";
            return false;
        }

        notification.Status = NotificationStatus.Sent;
        notification.SentDate = DateTime.Now;
        return true;
    }

    private static string GenerateSubject(string eventType, string templateName)
    {
        return eventType.ToLower() switch
        {
            "published" => $"Questionnaire Published: {templateName}",
            "unpublished" => $"Questionnaire Unpublished: {templateName}",
            "scheduled" => $"Questionnaire Scheduled: {templateName}",
            "scheduledpublish" => $"Scheduled Questionnaire Published: {templateName}",
            "approvalrequested" => $"Approval Requested: {templateName}",
            "approved" => $"Approval Granted: {templateName}",
            "rejected" => $"Approval Rejected: {templateName}",
            _ => $"Questionnaire Update: {templateName}"
        };
    }

    private static string GenerateMessage(string template, string templateName, string eventType)
    {
        var message = template.Replace("{TemplateName}", templateName);

        message += "\n\nThis is an automated notification from the Questionnaire Management System.";
        message += $"\nEvent: {eventType}";
        message += $"\nTimestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        return message;
    }

    private static string ExtractNameFromEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "Unknown";

        var localPart = email.Split('@')[0];
        return char.ToUpper(localPart[0]) + localPart[1..].ToLower();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}