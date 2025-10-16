using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Employee-centric questionnaire service for self-service operations.
///
/// PURPOSE AND SCOPE:
/// - This service provides operations from the EMPLOYEE'S PERSPECTIVE
/// - All operations are for the currently authenticated employee's own questionnaires
/// - Uses secure "me" endpoints where the backend resolves employee ID from UserContext
/// - Enforces implicit authorization - employees can only access their own data
///
/// WHY SEPARATE FROM IQuestionnaireResponseService?
///
/// 1. SECURITY MODEL:
///    - This service: Implicit "me" endpoints (employee ID from auth context)
///    - Response service: Explicit IDs with role-based authorization checks
///    - Mixing these authorization patterns would create security confusion
///
/// 2. USER PERSONA:
///    - This service: "What can I do with MY questionnaires?"
///    - Response service: "What can administrators do with ALL responses?"
///    - Clear separation by user role and intent
///
/// 3. SINGLE RESPONSIBILITY:
///    - This service: Employee self-service operations only
///    - Response service: Administrative response management + analytics
///    - Each service has focused, non-overlapping responsibilities
///
/// 4. BACKEND ALIGNMENT:
///    - Maps to /q/api/v1/employees/me/* and /c/api/v1/employees/me/* endpoints
///    - Response service maps to /q/api/v1/responses/* and /c/api/v1/responses/*
///    - Frontend services mirror backend controller structure
///
/// TYPICAL USERS: Employees filling out their own questionnaires
/// </summary>
public interface IEmployeeQuestionnaireService
{
    /// <summary>
    /// Gets all questionnaire assignments for the currently authenticated employee.
    /// Backend resolves employee ID from UserContext for security.
    /// </summary>
    Task<List<QuestionnaireAssignment>> GetMyAssignmentsAsync();

    /// <summary>
    /// Gets a specific questionnaire assignment for the currently authenticated employee.
    /// Validates that the assignment belongs to the authenticated employee.
    /// </summary>
    Task<QuestionnaireAssignment?> GetMyAssignmentByIdAsync(Guid assignmentId);

    /// <summary>
    /// Gets the questionnaire response for a specific assignment for the currently authenticated employee.
    /// Only returns responses that belong to the authenticated employee.
    /// </summary>
    Task<QuestionnaireResponse?> GetMyResponseAsync(Guid assignmentId);

    /// <summary>
    /// Saves the currently authenticated employee's response to their assigned questionnaire.
    /// Employee ID is resolved from UserContext on the backend for security.
    /// Stores responses with CompletionRole.Employee in the domain model.
    /// </summary>
    Task<QuestionnaireResponse> SaveMyResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses);

    /// <summary>
    /// Submits the currently authenticated employee's response for their assigned questionnaire.
    /// Marks the response as completed and triggers any workflow transitions.
    /// </summary>
    Task<QuestionnaireResponse?> SubmitMyResponseAsync(Guid assignmentId);

    /// <summary>
    /// Gets assignments filtered by workflow state for the currently authenticated employee.
    /// </summary>
    Task<List<QuestionnaireAssignment>> GetAssignmentsByWorkflowStateAsync(WorkflowState workflowState);

    /// <summary>
    /// Gets progress information for a specific assignment for the currently authenticated employee.
    /// </summary>
    Task<AssignmentProgress> GetAssignmentProgressAsync(Guid assignmentId);

    /// <summary>
    /// Gets progress information for all assignments for the currently authenticated employee.
    /// </summary>
    Task<List<AssignmentProgress>> GetAllAssignmentProgressAsync();
}

public class AssignmentProgress
{
    public Guid AssignmentId { get; set; }
    public int ProgressPercentage { get; set; }
    public int TotalQuestions { get; set; }
    public int AnsweredQuestions { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsCompleted { get; set; }
    public TimeSpan? TimeSpent { get; set; }
}