using ti8m.BeachBreak.CommandApi.Models;

namespace ti8m.BeachBreak.CommandApi.Services;

public interface IQuestionnaireService
{
    // Template management
    Task<List<QuestionnaireTemplate>> GetAllTemplatesAsync();
    Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid id);
    Task<QuestionnaireTemplate> CreateTemplateAsync(CreateQuestionnaireTemplateRequest request);
    Task<QuestionnaireTemplate?> UpdateTemplateAsync(Guid id, UpdateQuestionnaireTemplateRequest request);
    Task<bool> DeleteTemplateAsync(Guid id);
    Task<List<QuestionnaireTemplate>> GetTemplatesByCategoryAsync(string category);
    
    // Assignment management
    Task<List<QuestionnaireAssignment>> GetAllAssignmentsAsync();
    Task<QuestionnaireAssignment?> GetAssignmentByIdAsync(Guid id);
    Task<List<QuestionnaireAssignment>> GetAssignmentsByEmployeeAsync(string employeeId);
    Task<List<QuestionnaireAssignment>> CreateAssignmentsAsync(CreateAssignmentRequest request);
    Task<QuestionnaireAssignment?> UpdateAssignmentStatusAsync(Guid id, AssignmentStatus status);
    Task<bool> DeleteAssignmentAsync(Guid id);
    
    // Response management
    Task<List<QuestionnaireResponse>> GetAllResponsesAsync();
    Task<QuestionnaireResponse?> GetResponseByIdAsync(Guid id);
    Task<QuestionnaireResponse?> GetResponseByAssignmentIdAsync(Guid assignmentId);
    Task<QuestionnaireResponse> CreateOrUpdateResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses);
    Task<QuestionnaireResponse?> SubmitResponseAsync(Guid assignmentId);
    
    // Analytics
    Task<Dictionary<string, object>> GetTemplateAnalyticsAsync(Guid templateId);
    Task<Dictionary<string, object>> GetOverallAnalyticsAsync();
}