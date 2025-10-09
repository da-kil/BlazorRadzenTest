using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Administrative questionnaire response service for managing all responses across the system.
///
/// PURPOSE AND SCOPE:
/// - This service provides ADMINISTRATIVE operations from the MANAGER/HR/ADMIN PERSPECTIVE
/// - Handles operations on responses across multiple employees and assignments
/// - Uses explicit IDs with role-based authorization checks on the backend
/// - Provides analytics, reporting, and bulk operations capabilities
///
/// WHY SEPARATE FROM IEmployeeQuestionnaireService?
///
/// 1. SECURITY MODEL:
///    - This service: Explicit IDs with role-based authorization (Manager/HR/Admin only)
///    - Employee service: Implicit "me" endpoints (employee ID from auth context)
///    - Different authorization patterns prevent mixing employee and admin operations
///
/// 2. USER PERSONA:
///    - This service: "What can ADMINISTRATORS do with ALL responses?"
///    - Employee service: "What can I do with MY questionnaires?"
///    - Clear separation prevents privilege escalation and maintains security boundaries
///
/// 3. SINGLE RESPONSIBILITY:
///    - This service: Administrative response management, analytics, and reporting
///    - Employee service: Employee self-service operations only
///    - Each service focuses on distinct operational domains
///
/// 4. BACKEND ALIGNMENT:
///    - Maps to /q/api/v1/responses/* and /c/api/v1/responses/* endpoints
///    - Employee service maps to /q/api/v1/employees/me/* and /c/api/v1/employees/me/*
///    - Frontend services mirror backend controller structure
///
/// 5. FUNCTIONALITY SCOPE:
///    - This service includes: Analytics, cross-employee queries, manager responses, bulk operations
///    - Employee service includes: Only personal assignment and response operations
///    - Administrative features don't belong in employee-facing service
///
/// KEY METHODS:
/// - SaveManagerResponseAsync: Manager provides feedback (stores with CompletionRole.Manager)
/// - GetResponsesByEmployeeAsync: View all responses for any employee (admin/manager)
/// - Analytics methods: System-wide response analytics and reporting
///
/// TYPICAL USERS: Managers reviewing team questionnaires, HR/Admin generating reports
/// </summary>
public interface IQuestionnaireResponseService
{
    // ============================================================================
    // RESPONSE CRUD OPERATIONS (Admin/Manager access)
    // ============================================================================

    /// <summary>
    /// Gets all questionnaire responses in the system.
    /// Requires elevated permissions (Manager/HR/Admin).
    /// </summary>
    Task<List<QuestionnaireResponse>> GetAllResponsesAsync();

    /// <summary>
    /// Gets a specific questionnaire response by its ID.
    /// Subject to authorization checks based on user role.
    /// </summary>
    Task<QuestionnaireResponse?> GetResponseByIdAsync(Guid id);

    /// <summary>
    /// Gets the questionnaire response for a specific assignment.
    /// Used by managers to view employee responses.
    /// Subject to authorization checks (manager must manage the employee).
    /// </summary>
    Task<QuestionnaireResponse?> GetResponseByAssignmentIdAsync(Guid assignmentId);

    /// <summary>
    /// Deletes a questionnaire response.
    /// Requires admin permissions. Used for data cleanup or corrections.
    /// </summary>
    Task<bool> DeleteResponseAsync(Guid id);

    // ============================================================================
    // RESPONSE MANAGEMENT (Role-specific operations)
    // ============================================================================

    /// <summary>
    /// Saves a response for any assignment (generic save operation).
    /// Typically used for administrative corrections or bulk operations.
    /// Requires appropriate authorization based on the assignment.
    /// </summary>
    Task<QuestionnaireResponse> SaveResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses);

    /// <summary>
    /// Saves a manager's feedback/responses for an employee's questionnaire.
    /// Manager ID is resolved from UserContext on the backend.
    /// Stores responses with CompletionRole.Manager in the domain model.
    /// Backend validates that the manager is authorized for this assignment.
    /// </summary>
    Task<QuestionnaireResponse> SaveManagerResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses);

    /// <summary>
    /// Submits a response for a specific assignment (generic submit).
    /// Typically used for administrative workflows.
    /// Subject to authorization checks.
    /// </summary>
    Task<QuestionnaireResponse?> SubmitResponseAsync(Guid assignmentId);

    // ============================================================================
    // RESPONSE QUERIES (Cross-employee, administrative access)
    // ============================================================================

    /// <summary>
    /// Gets all questionnaire responses for a specific employee.
    /// Used by managers to view their direct reports' responses.
    /// Used by HR/Admin for reporting and analytics.
    /// Subject to authorization checks.
    /// </summary>
    Task<List<QuestionnaireResponse>> GetResponsesByEmployeeAsync(string employeeId);

    /// <summary>
    /// Gets all responses for a specific questionnaire template.
    /// Used for template effectiveness analysis and reporting.
    /// Requires Manager/HR/Admin permissions.
    /// </summary>
    Task<List<QuestionnaireResponse>> GetResponsesByTemplateAsync(Guid templateId);

    /// <summary>
    /// Gets all responses filtered by completion status.
    /// Used for workflow management and progress tracking.
    /// Requires Manager/HR/Admin permissions.
    /// </summary>
    Task<List<QuestionnaireResponse>> GetResponsesByStatusAsync(ResponseStatus status);

    // ============================================================================
    // RESPONSE ANALYTICS (HR/Admin reporting)
    // ============================================================================

    /// <summary>
    /// Gets overall response analytics across all questionnaires.
    /// Used for system-wide metrics and KPIs.
    /// Requires HR/Admin permissions.
    /// </summary>
    Task<Dictionary<string, object>> GetResponseAnalyticsAsync();

    /// <summary>
    /// Gets response statistics for a specific employee.
    /// Used by managers for employee performance reviews.
    /// Subject to authorization checks.
    /// </summary>
    Task<Dictionary<string, object>> GetEmployeeResponseStatsAsync(string employeeId);

    /// <summary>
    /// Gets response statistics for a specific questionnaire template.
    /// Used for template effectiveness analysis.
    /// Requires Manager/HR/Admin permissions.
    /// </summary>
    Task<Dictionary<string, object>> GetTemplateResponseStatsAsync(Guid templateId);

    /// <summary>
    /// Gets overall system analytics including completion rates, trends, etc.
    /// Used for executive dashboards and strategic planning.
    /// Requires HR/Admin permissions.
    /// </summary>
    Task<Dictionary<string, object>> GetOverallAnalyticsAsync();
}