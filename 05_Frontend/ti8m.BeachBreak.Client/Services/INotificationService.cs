using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface INotificationService
{
    // Stakeholder Notifications
    Task<bool> SendPublishNotificationAsync(Guid templateId, string templateName, string eventType, List<string> stakeholderEmails);
    Task<List<StakeholderNotification>> GetNotificationHistoryAsync(Guid templateId);
    Task<List<StakeholderNotification>> GetPendingNotificationsAsync();

    // Notification Templates
    Task<string> GetNotificationTemplateAsync(string eventType);
    Task<bool> ConfigureStakeholdersAsync(Guid templateId, List<string> stakeholderEmails);
    Task<List<string>> GetStakeholdersAsync(Guid templateId);

    // Notification Processing
    Task<bool> ProcessPendingNotificationsAsync();
    Task<StakeholderNotification?> RetryFailedNotificationAsync(Guid notificationId);
}