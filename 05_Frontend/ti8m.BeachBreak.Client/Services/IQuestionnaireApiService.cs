using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface IQuestionnaireApiService
{
    // Template management
    Task<List<QuestionnaireTemplate>> GetAllTemplatesAsync();
    Task<List<QuestionnaireTemplate>> GetAllPublishedTemplatesAsync();
    Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid id);
    Task<QuestionnaireTemplate> CreateTemplateAsync(QuestionnaireTemplate template);
    Task<QuestionnaireTemplate?> UpdateTemplateAsync(QuestionnaireTemplate template);
    Task<bool> DeleteTemplateAsync(Guid id);
    Task<List<QuestionnaireTemplate>> GetTemplatesByCategoryAsync(string category);

    // Enhanced status-specific template queries
    Task<List<QuestionnaireTemplate>> GetPublishedTemplatesAsync();
    Task<List<QuestionnaireTemplate>> GetDraftTemplatesAsync();
    Task<List<QuestionnaireTemplate>> GetAssignableTemplatesAsync(); // Active + Published
    Task<List<QuestionnaireTemplate>> GetActiveTemplatesAsync();
    Task<List<QuestionnaireTemplate>> GetInactiveTemplatesAsync();

    // Publishing operations
    Task<QuestionnaireTemplate?> PublishTemplateAsync(Guid templateId, string publishedBy);
    Task<QuestionnaireTemplate?> UnpublishTemplateAsync(Guid templateId);
    Task<QuestionnaireTemplate?> ActivateTemplateAsync(Guid templateId);
    Task<QuestionnaireTemplate?> DeactivateTemplateAsync(Guid templateId);
    
    // Assignment management
    Task<List<QuestionnaireAssignment>> GetAllAssignmentsAsync();
    Task<QuestionnaireAssignment?> GetAssignmentByIdAsync(Guid id);
    Task<List<QuestionnaireAssignment>> GetAssignmentsByEmployeeAsync(string employeeId);
    Task<List<QuestionnaireAssignment>> CreateAssignmentsAsync(Guid templateId, List<string> employeeIds, DateTime? dueDate, string? notes, string assignedBy);
    Task<QuestionnaireAssignment?> UpdateAssignmentStatusAsync(Guid id, AssignmentStatus status);
    Task<bool> DeleteAssignmentAsync(Guid id);
    
    // Response management
    Task<List<QuestionnaireResponse>> GetAllResponsesAsync();
    Task<QuestionnaireResponse?> GetResponseByIdAsync(Guid id);
    Task<QuestionnaireResponse?> GetResponseByAssignmentIdAsync(Guid assignmentId);
    Task<QuestionnaireResponse> SaveResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses);
    Task<QuestionnaireResponse?> SubmitResponseAsync(Guid assignmentId);
    
    // Analytics
    Task<Dictionary<string, object>> GetTemplateAnalyticsAsync(Guid templateId);
    Task<Dictionary<string, object>> GetOverallAnalyticsAsync();
    Task<Dictionary<string, object>> GetPublishingAnalyticsAsync();
}