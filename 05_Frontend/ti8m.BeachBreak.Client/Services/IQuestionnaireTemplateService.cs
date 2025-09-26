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
    Task<List<QuestionnaireTemplate>> GetInactiveTemplatesAsync();
    Task<List<QuestionnaireTemplate>> GetAllPublishedTemplatesAsync();

    // Template status operations
    Task<QuestionnaireTemplate?> PublishTemplateAsync(Guid templateId, string publishedBy);
    Task<QuestionnaireTemplate?> UnpublishTemplateAsync(Guid templateId);
    Task<QuestionnaireTemplate?> ActivateTemplateAsync(Guid templateId);
    Task<QuestionnaireTemplate?> DeactivateTemplateAsync(Guid templateId);

    // Template analytics
    Task<Dictionary<string, object>> GetTemplateAnalyticsAsync(Guid templateId);
    Task<Dictionary<string, object>> GetPublishingAnalyticsAsync();
}