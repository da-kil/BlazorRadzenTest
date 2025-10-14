using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto;

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
    Task<List<QuestionnaireAssignment>> CreateAssignmentsAsync(Guid templateId, List<EmployeeDto> employees, DateTime? dueDate, string? notes);
    Task<List<QuestionnaireAssignment>> CreateManagerAssignmentsAsync(Guid templateId, List<EmployeeDto> employees, DateTime? dueDate, string? notes);
    Task<QuestionnaireAssignment?> UpdateAssignmentWorkflowStateAsync(Guid id, WorkflowState workflowState);

    // Assignment queries
    Task<List<QuestionnaireAssignment>> GetAssignmentsByEmployeeAsync(Guid employeeId);
    Task<List<QuestionnaireAssignment>> GetAssignmentsByTemplateAsync(Guid templateId);
    Task<List<QuestionnaireAssignment>> GetAssignmentsByWorkflowStateAsync(WorkflowState workflowState);
    Task<List<QuestionnaireAssignment>> GetAssignmentsByAssignerAsync(string assignerId);

    // Assignment analytics
    Task<Dictionary<string, object>> GetAssignmentAnalyticsAsync();
    Task<Dictionary<string, object>> GetEmployeeAssignmentStatsAsync(Guid employeeId);
    Task<Dictionary<string, object>> GetTemplateAssignmentStatsAsync(Guid templateId);

    // Workflow operations
    Task<bool> CompleteSectionAsEmployeeAsync(Guid assignmentId, Guid sectionId);
    Task<bool> CompleteBulkSectionsAsEmployeeAsync(Guid assignmentId, List<Guid> sectionIds);
    Task<bool> CompleteSectionAsManagerAsync(Guid assignmentId, Guid sectionId);
    Task<bool> CompleteBulkSectionsAsManagerAsync(Guid assignmentId, List<Guid> sectionIds);
    Task<bool> SubmitEmployeeQuestionnaireAsync(Guid assignmentId, string submittedBy);
    Task<bool> SubmitManagerQuestionnaireAsync(Guid assignmentId, string submittedBy);
    Task<bool> InitiateReviewAsync(Guid assignmentId, string initiatedBy);
    Task<bool> ConfirmManagerReviewAsync(Guid assignmentId, string confirmedBy);
    Task<bool> EditAnswerDuringReviewAsync(Guid assignmentId, Guid sectionId, Guid questionId, CompletionRole originalCompletionRole, string answer, string editedBy);
    Task<bool> ConfirmEmployeeReviewAsync(Guid assignmentId, string confirmedBy);
    Task<bool> FinalizeQuestionnaireAsync(Guid assignmentId, string finalizedBy);

    // Review changes tracking
    Task<List<ReviewChangeDto>> GetReviewChangesAsync(Guid assignmentId);
}