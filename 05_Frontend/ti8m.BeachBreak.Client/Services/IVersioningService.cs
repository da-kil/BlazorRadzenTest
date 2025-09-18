using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface IVersioningService
{
    // Version Management
    Task<TemplateVersion?> CreateVersionAsync(Guid templateId, string changeDescription, TemplateVersionType versionType);
    Task<List<TemplateVersion>> GetVersionHistoryAsync(Guid templateId);
    Task<TemplateVersion?> GetVersionAsync(Guid versionId);
    Task<QuestionnaireTemplate?> GetTemplateAtVersionAsync(Guid versionId);
    Task<bool> RevertToVersionAsync(Guid templateId, Guid versionId);
    Task<string> GetVersionDiffAsync(Guid fromVersionId, Guid toVersionId);

    // Publish History
    Task<List<PublishHistory>> GetPublishHistoryAsync(Guid templateId);
    Task<PublishHistory?> RecordPublishActionAsync(Guid templateId, PublishHistoryAction action, string performedBy, string notes = "", bool wasScheduled = false);
}