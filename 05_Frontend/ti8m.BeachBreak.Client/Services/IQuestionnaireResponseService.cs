using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service interface for managing questionnaire responses
/// </summary>
public interface IQuestionnaireResponseService
{
    // Response CRUD operations
    Task<List<QuestionnaireResponse>> GetAllResponsesAsync();
    Task<QuestionnaireResponse?> GetResponseByIdAsync(Guid id);
    Task<QuestionnaireResponse?> GetResponseByAssignmentIdAsync(Guid assignmentId);

    // Response management
    Task<QuestionnaireResponse> SaveResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses);
    Task<QuestionnaireResponse?> SubmitResponseAsync(Guid assignmentId);
    Task<bool> DeleteResponseAsync(Guid id);

    // Response queries
    Task<List<QuestionnaireResponse>> GetResponsesByEmployeeAsync(string employeeId);
    Task<List<QuestionnaireResponse>> GetResponsesByTemplateAsync(Guid templateId);
    Task<List<QuestionnaireResponse>> GetResponsesByStatusAsync(ResponseStatus status);

    // Response analytics
    Task<Dictionary<string, object>> GetResponseAnalyticsAsync();
    Task<Dictionary<string, object>> GetEmployeeResponseStatsAsync(string employeeId);
    Task<Dictionary<string, object>> GetTemplateResponseStatsAsync(Guid templateId);
    Task<Dictionary<string, object>> GetOverallAnalyticsAsync();
}