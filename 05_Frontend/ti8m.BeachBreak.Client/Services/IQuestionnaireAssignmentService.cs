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
    Task<QuestionnaireAssignment?> GetMyAssignmentByIdAsync(Guid id);
    Task<bool> DeleteAssignmentAsync(Guid id);

    // Assignment creation and management
    Task<List<QuestionnaireAssignment>> CreateAssignmentsAsync(Guid templateId, QuestionnaireProcessType processType, List<EmployeeDto> employees, DateTime? dueDate, string? notes);
    Task<List<QuestionnaireAssignment>> CreateManagerAssignmentsAsync(Guid templateId, QuestionnaireProcessType processType, List<EmployeeDto> employees, DateTime? dueDate, string? notes);
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
    Task<bool> InitializeAssignmentAsync(Guid assignmentId, string? initializationNotes);
    Task<bool> AddCustomSectionsAsync(Guid assignmentId, AddCustomSectionsDto dto);
    Task<List<QuestionSection>> GetCustomSectionsAsync(Guid assignmentId);
    Task<List<QuestionSection>> GetMyCustomSectionsAsync(Guid assignmentId);
    Task<bool> SubmitEmployeeQuestionnaireAsync(Guid assignmentId, string submittedBy);
    Task<bool> SubmitManagerQuestionnaireAsync(Guid assignmentId, string submittedBy);
    Task<bool> InitiateReviewAsync(Guid assignmentId, string initiatedBy);
    Task<bool> FinishReviewMeetingAsync(Guid assignmentId, string finishedBy, string? reviewSummary);
    Task<bool> EditAnswerDuringReviewAsync(Guid assignmentId, Guid sectionId, Guid questionId, CompletionRole originalCompletionRole, string answer, string editedBy);
    Task<bool> EditGoalDuringReviewAsync(Guid assignmentId, Guid sectionId, Guid questionId, CompletionRole originalCompletionRole, string goalJson, string editedBy);
    Task<bool> EditGoalAsync(Guid assignmentId, Guid goalId, EditGoalDto editDto);
    Task<bool> ConfirmEmployeeReviewAsync(Guid assignmentId, string confirmedBy, string? comments);
    Task<bool> FinalizeQuestionnaireAsync(Guid assignmentId, string finalizedBy, string? finalNotes);

    // InReview note management
    Task<Result<Guid>> AddInReviewNoteAsync(Guid assignmentId, string content, Guid? sectionId);
    Task<bool> UpdateInReviewNoteAsync(Guid assignmentId, Guid noteId, string content);
    Task<bool> DeleteInReviewNoteAsync(Guid assignmentId, Guid noteId);

    // Review changes tracking
    Task<List<ReviewChangeDto>> GetReviewChangesAsync(Guid assignmentId);

    // Workflow reopening
    Task<bool> ReopenQuestionnaireAsync(Guid assignmentId, WorkflowState targetState, string reopenReason);
}