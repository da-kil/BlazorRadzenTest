using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class SchedulingService : ISchedulingService
{
    private readonly IQuestionnaireApiService _questionnaireService;
    private readonly IVersioningService _versioningService;

    // In-memory storage for demo purposes - would be database in real implementation
    private readonly List<PublishSchedule> _schedules = new();

    public SchedulingService(IQuestionnaireApiService questionnaireService, IVersioningService versioningService)
    {
        _questionnaireService = questionnaireService;
        _versioningService = versioningService;
    }

    public async Task<PublishSchedule?> SchedulePublishAsync(Guid templateId, DateTime publishTime, string notes = "", DateTime? unpublishTime = null)
    {
        try
        {
            // Validate that template exists
            var template = await _questionnaireService.GetTemplateByIdAsync(templateId);
            if (template == null) return null;

            // Validate publish time is in the future
            if (publishTime <= DateTime.Now)
                return null;

            // Cancel any existing pending schedules for this template
            var existingSchedules = _schedules.Where(s => s.TemplateId == templateId && s.Status == PublishScheduleStatus.Pending).ToList();
            foreach (var existing in existingSchedules)
            {
                existing.Status = PublishScheduleStatus.Cancelled;
            }

            var schedule = new PublishSchedule
            {
                TemplateId = templateId,
                ScheduledPublishTime = publishTime,
                ScheduledBy = "Current User", // Would get from authentication context
                Notes = notes,
                ScheduledUnpublishTime = unpublishTime
            };

            _schedules.Add(schedule);

            // Record the scheduling action
            await _versioningService.RecordPublishActionAsync(templateId, PublishHistoryAction.ScheduledPublish,
                "Current User", $"Scheduled for {publishTime:yyyy-MM-dd HH:mm}", true);

            return schedule;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<PublishSchedule>> GetScheduledPublishesAsync(Guid? templateId = null)
    {
        await Task.Delay(50);

        var query = _schedules.AsQueryable();

        if (templateId.HasValue)
            query = query.Where(s => s.TemplateId == templateId.Value);

        return query.OrderBy(s => s.ScheduledPublishTime).ToList();
    }

    public async Task<bool> CancelScheduleAsync(Guid scheduleId)
    {
        await Task.Delay(50);

        var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
        if (schedule == null || schedule.Status != PublishScheduleStatus.Pending)
            return false;

        schedule.Status = PublishScheduleStatus.Cancelled;

        // Record the cancellation
        await _versioningService.RecordPublishActionAsync(schedule.TemplateId, PublishHistoryAction.CancelledSchedule,
            "Current User", $"Cancelled scheduled publish for {schedule.ScheduledPublishTime:yyyy-MM-dd HH:mm}");

        return true;
    }

    public async Task<bool> UpdateScheduleAsync(Guid scheduleId, DateTime newPublishTime, string notes = "")
    {
        await Task.Delay(50);

        var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
        if (schedule == null || schedule.Status != PublishScheduleStatus.Pending)
            return false;

        if (newPublishTime <= DateTime.Now)
            return false;

        var oldTime = schedule.ScheduledPublishTime;
        schedule.ScheduledPublishTime = newPublishTime;
        schedule.Notes = notes;

        // Record the update
        await _versioningService.RecordPublishActionAsync(schedule.TemplateId, PublishHistoryAction.ScheduledPublish,
            "Current User", $"Rescheduled from {oldTime:yyyy-MM-dd HH:mm} to {newPublishTime:yyyy-MM-dd HH:mm}", true);

        return true;
    }

    public async Task<List<PublishSchedule>> GetPendingSchedulesAsync()
    {
        await Task.Delay(50);

        return _schedules.Where(s => s.Status == PublishScheduleStatus.Pending &&
                                    s.ScheduledPublishTime <= DateTime.Now)
                        .OrderBy(s => s.ScheduledPublishTime)
                        .ToList();
    }

    public async Task<bool> ExecuteScheduleAsync(Guid scheduleId)
    {
        try
        {
            var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
            if (schedule == null || schedule.Status != PublishScheduleStatus.Pending)
                return false;

            // Get the template
            var template = await _questionnaireService.GetTemplateByIdAsync(schedule.TemplateId);
            if (template == null)
            {
                schedule.Status = PublishScheduleStatus.Failed;
                schedule.ErrorMessage = "Template not found";
                schedule.ExecutedDate = DateTime.Now;
                return false;
            }

            // Execute the publish
            template.IsPublished = true;
            template.PublishedDate ??= DateTime.Now;
            template.LastPublishedDate = DateTime.Now;
            template.PublishedBy = schedule.ScheduledBy;

            var updated = await _questionnaireService.UpdateTemplateAsync(template);
            if (updated != null)
            {
                schedule.Status = PublishScheduleStatus.Executed;
                schedule.ExecutedDate = DateTime.Now;
                schedule.ExecutionLog = $"Successfully published at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                // Record the actual publish action
                await _versioningService.RecordPublishActionAsync(schedule.TemplateId, PublishHistoryAction.Published,
                    schedule.ScheduledBy, "Executed scheduled publish", true);

                return true;
            }
            else
            {
                schedule.Status = PublishScheduleStatus.Failed;
                schedule.ErrorMessage = "Failed to update template";
                schedule.ExecutedDate = DateTime.Now;
                return false;
            }
        }
        catch (Exception ex)
        {
            var schedule = _schedules.FirstOrDefault(s => s.Id == scheduleId);
            if (schedule != null)
            {
                schedule.Status = PublishScheduleStatus.Failed;
                schedule.ErrorMessage = ex.Message;
                schedule.ExecutedDate = DateTime.Now;
            }
            return false;
        }
    }
}