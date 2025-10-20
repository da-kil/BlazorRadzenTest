using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service interface for managing questionnaire templates
/// </summary>
public interface IQuestionnaireTemplateService
{
    // Template CRUD operations
    Task<List<QuestionnaireTemplate>> GetAllTemplatesAsync();
    Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid id);
    Task<QuestionnaireTemplate> CreateTemplateAsync(QuestionnaireTemplate template);
    Task<QuestionnaireTemplate?> UpdateTemplateAsync(QuestionnaireTemplate template);
    Task<bool> DeleteTemplateAsync(Guid id);

    // Template filtering and queries
    Task<List<QuestionnaireTemplate>> GetTemplatesByCategoryAsync(string category);
    Task<List<QuestionnaireTemplate>> GetPublishedTemplatesAsync();
    Task<List<QuestionnaireTemplate>> GetDraftTemplatesAsync();
    Task<List<QuestionnaireTemplate>> GetAssignableTemplatesAsync(); // Active + Published
    Task<List<QuestionnaireTemplate>> GetActiveTemplatesAsync();
    Task<List<QuestionnaireTemplate>> GetArchivedTemplatesAsync();
    Task<List<QuestionnaireTemplate>> GetAllPublishedTemplatesAsync();

    // Template status operations
    Task<QuestionnaireTemplate?> PublishTemplateAsync(Guid templateId);
    Task<QuestionnaireTemplate?> UnpublishTemplateAsync(Guid templateId);
    Task<QuestionnaireTemplate?> ArchiveTemplateAsync(Guid templateId);
    Task<QuestionnaireTemplate?> RestoreTemplateAsync(Guid templateId);

    // Template cloning
    Task<Guid?> CloneTemplateAsync(Guid templateId, string? namePrefix = null);

    // Template analytics
    Task<Dictionary<string, object>> GetTemplateAnalyticsAsync(Guid templateId);
    Task<Dictionary<string, object>> GetPublishingAnalyticsAsync();
}