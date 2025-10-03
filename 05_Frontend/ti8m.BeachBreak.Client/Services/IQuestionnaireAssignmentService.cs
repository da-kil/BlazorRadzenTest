using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service interface for managing questionnaire assignments
/// </summary>
public interface IQuestionnaireAssignmentService
{
    // Assignment CRUD operations
    Task<List<QuestionnaireAssignment>> GetAllAssignmentsAsync();
    Task<QuestionnaireAssignment?> GetAssignmentByIdAsync(Guid id);
    Task<bool> DeleteAssignmentAsync(Guid id);

    // Assignment creation and management
    Task<List<QuestionnaireAssignment>> CreateAssignmentsAsync(Guid templateId, List<EmployeeDto> employees, DateTime? dueDate, string? notes, string assignedBy);
    Task<QuestionnaireAssignment?> UpdateAssignmentStatusAsync(Guid id, AssignmentStatus status);

    // Assignment queries
    Task<List<QuestionnaireAssignment>> GetAssignmentsByEmployeeAsync(Guid employeeId);
    Task<List<QuestionnaireAssignment>> GetAssignmentsByTemplateAsync(Guid templateId);
    Task<List<QuestionnaireAssignment>> GetAssignmentsByStatusAsync(AssignmentStatus status);
    Task<List<QuestionnaireAssignment>> GetAssignmentsByAssignerAsync(string assignerId);

    // Assignment analytics
    Task<Dictionary<string, object>> GetAssignmentAnalyticsAsync();
    Task<Dictionary<string, object>> GetEmployeeAssignmentStatsAsync(Guid employeeId);
    Task<Dictionary<string, object>> GetTemplateAssignmentStatsAsync(Guid templateId);

    // Workflow operations
    Task<bool> CompleteSectionAsEmployeeAsync(Guid assignmentId, Guid sectionId);
    Task<bool> CompleteSectionAsManagerAsync(Guid assignmentId, Guid sectionId);
    Task<bool> ConfirmEmployeeCompletionAsync(Guid assignmentId, string confirmedBy);
    Task<bool> ConfirmManagerCompletionAsync(Guid assignmentId, string confirmedBy);
    Task<bool> InitiateReviewAsync(Guid assignmentId, string initiatedBy);
    Task<bool> EditAnswerDuringReviewAsync(Guid assignmentId, Guid sectionId, Guid questionId, string answer, string editedBy);
    Task<bool> ConfirmEmployeeReviewAsync(Guid assignmentId, string confirmedBy);
    Task<bool> FinalizeQuestionnaireAsync(Guid assignmentId, string finalizedBy);
}