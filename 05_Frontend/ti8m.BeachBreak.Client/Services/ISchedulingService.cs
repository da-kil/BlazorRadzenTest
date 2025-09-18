using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface ISchedulingService
{
    // Schedule Management
    Task<PublishSchedule?> SchedulePublishAsync(Guid templateId, DateTime publishTime, string notes = "", DateTime? unpublishTime = null);
    Task<List<PublishSchedule>> GetScheduledPublishesAsync(Guid? templateId = null);
    Task<bool> CancelScheduleAsync(Guid scheduleId);
    Task<bool> UpdateScheduleAsync(Guid scheduleId, DateTime newPublishTime, string notes = "");

    // Background execution simulation (would be handled by a background service in real implementation)
    Task<List<PublishSchedule>> GetPendingSchedulesAsync();
    Task<bool> ExecuteScheduleAsync(Guid scheduleId);
}