using Microsoft.Extensions.Logging;

namespace ti8m.BeachBreak.Core.Infrastructure;

public static partial class LoggerMessageDefinitions
{
    [LoggerMessage(
    EventId = 4001,
    Level = LogLevel.Information,
    Message = "Loading events for aggregate `{Id}`.")]
    public static partial void LogLoadEventStream(this ILogger logger, Guid id);

    [LoggerMessage(
    EventId = 4002,
    Level = LogLevel.Information,
    Message = "Saving events (count: {NumberOfEvents}) for aggregate `{Id}`.")]
    public static partial void LogSaveEventStream(this ILogger logger, long numberOfEvents, Guid id);

    // Authorization logging (4100-4199)
    [LoggerMessage(
    EventId = 4100,
    Level = LogLevel.Warning,
    Message = "Authorization failed: User ID not found in claims")]
    public static partial void LogAuthorizationFailedNoUserId(this ILogger logger);

    [LoggerMessage(
    EventId = 4101,
    Level = LogLevel.Warning,
    Message = "Authorization failed: Employee not found for user ID {UserId}")]
    public static partial void LogAuthorizationFailedEmployeeNotFound(this ILogger logger, Guid userId);

    [LoggerMessage(
    EventId = 4102,
    Level = LogLevel.Warning,
    Message = "Authorization failed: User {UserId} with role {UserRole} lacks required permissions for policies [{RequiredPolicies}] on path {Path}")]
    public static partial void LogAuthorizationFailedInsufficientPermissions(this ILogger logger, Guid userId, string userRole, string requiredPolicies, string path);

    [LoggerMessage(
    EventId = 4103,
    Level = LogLevel.Information,
    Message = "Authorization succeeded: User {UserId} with role {UserRole} accessed path {Path}")]
    public static partial void LogAuthorizationSucceeded(this ILogger logger, Guid userId, string userRole, string path);

    [LoggerMessage(
    EventId = 4104,
    Level = LogLevel.Information,
    Message = "Authorization cache manually invalidated for employee {EmployeeId} by admin {AdminId}")]
    public static partial void LogAuthorizationCacheInvalidated(this ILogger logger, Guid employeeId, string adminId);

    // InReview note operations logging (7001-7010)
    [LoggerMessage(
    EventId = 7001,
    Level = LogLevel.Information,
    Message = "InReview note added for assignment {AssignmentId} by employee {EmployeeId} with note ID {NoteId}")]
    public static partial void InReviewNoteAdded(this ILogger logger, Guid assignmentId, Guid noteId, Guid employeeId, Exception? exception);

    [LoggerMessage(
    EventId = 7002,
    Level = LogLevel.Information,
    Message = "InReview note {NoteId} updated for assignment {AssignmentId} by employee {EmployeeId}")]
    public static partial void InReviewNoteUpdated(this ILogger logger, Guid noteId, Guid assignmentId, Guid employeeId, Exception? exception);

    [LoggerMessage(
    EventId = 7003,
    Level = LogLevel.Information,
    Message = "InReview note {NoteId} deleted for assignment {AssignmentId} by employee {EmployeeId}")]
    public static partial void InReviewNoteDeleted(this ILogger logger, Guid noteId, Guid assignmentId, Guid employeeId, Exception? exception);

    [LoggerMessage(
    EventId = 7004,
    Level = LogLevel.Error,
    Message = "Failed to add InReview note for assignment {AssignmentId}: {ErrorMessage}")]
    public static partial void AddInReviewNoteFailed(this ILogger logger, Guid assignmentId, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 7005,
    Level = LogLevel.Error,
    Message = "Failed to update InReview note {NoteId} for assignment {AssignmentId}: {ErrorMessage}")]
    public static partial void UpdateInReviewNoteFailed(this ILogger logger, Guid noteId, Guid assignmentId, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 7006,
    Level = LogLevel.Error,
    Message = "Failed to delete InReview note {NoteId} for assignment {AssignmentId}: {ErrorMessage}")]
    public static partial void DeleteInReviewNoteFailed(this ILogger logger, Guid noteId, Guid assignmentId, string errorMessage, Exception exception);

    [LoggerMessage(
    EventId = 7007,
    Level = LogLevel.Warning,
    Message = "Attempted to add InReview note for assignment {AssignmentId} in invalid state {WorkflowState}")]
    public static partial void InReviewNoteInvalidState(this ILogger logger, Guid assignmentId, string workflowState);

    [LoggerMessage(
    EventId = 7008,
    Level = LogLevel.Warning,
    Message = "User {UserId} attempted to modify InReview note {NoteId} without ownership permissions")]
    public static partial void InReviewNoteUnauthorizedModification(this ILogger logger, Guid userId, Guid noteId);

    // General endpoint operations logging (8001-8020)
    [LoggerMessage(
    EventId = 8001,
    Level = LogLevel.Error,
    Message = "Error processing request")]
    public static partial void LogEndpointError(this ILogger logger, Exception exception);

    [LoggerMessage(
    EventId = 8002,
    Level = LogLevel.Error,
    Message = "Error processing request in {EndpointName}")]
    public static partial void LogEndpointErrorWithName(this ILogger logger, string endpointName, Exception exception);

    // Category endpoints logging (8021-8040)
    [LoggerMessage(
    EventId = 8021,
    Level = LogLevel.Error,
    Message = "Error retrieving categories")]
    public static partial void LogCategoriesRetrievalError(this ILogger logger, Exception exception);

    [LoggerMessage(
    EventId = 8022,
    Level = LogLevel.Error,
    Message = "Error retrieving category {CategoryId}")]
    public static partial void LogCategoryRetrievalError(this ILogger logger, Guid categoryId, Exception exception);

    [LoggerMessage(
    EventId = 8023,
    Level = LogLevel.Error,
    Message = "Error creating category")]
    public static partial void LogCategoryCreationError(this ILogger logger, Exception exception);

    [LoggerMessage(
    EventId = 8024,
    Level = LogLevel.Error,
    Message = "Error updating category {CategoryId}")]
    public static partial void LogCategoryUpdateError(this ILogger logger, Guid categoryId, Exception exception);

    [LoggerMessage(
    EventId = 8025,
    Level = LogLevel.Error,
    Message = "Error deactivating category {CategoryId}")]
    public static partial void LogCategoryDeactivationError(this ILogger logger, Guid categoryId, Exception exception);

    // Translation endpoints logging (8041-8060)
    [LoggerMessage(
    EventId = 8041,
    Level = LogLevel.Debug,
    Message = "Getting translation for key: {Key} (user language: {Language})")]
    public static partial void LogGetTranslationDebug(this ILogger logger, string key, string language);

    [LoggerMessage(
    EventId = 8042,
    Level = LogLevel.Error,
    Message = "Error retrieving translation for key: {Key}")]
    public static partial void LogTranslationRetrievalError(this ILogger logger, string key, Exception exception);

    [LoggerMessage(
    EventId = 8043,
    Level = LogLevel.Information,
    Message = "Getting all translations - API called")]
    public static partial void LogGetAllTranslationsInfo(this ILogger logger);

    [LoggerMessage(
    EventId = 8044,
    Level = LogLevel.Information,
    Message = "Retrieved {Count} translations from service")]
    public static partial void LogRetrievedTranslationsCount(this ILogger logger, int count);

    [LoggerMessage(
    EventId = 8045,
    Level = LogLevel.Information,
    Message = "Sample translation: Key='{Key}', English='{English}', German='{German}'")]
    public static partial void LogSampleTranslation(this ILogger logger, string key, string english, string german);

    [LoggerMessage(
    EventId = 8046,
    Level = LogLevel.Error,
    Message = "Error retrieving all translations")]
    public static partial void LogAllTranslationsRetrievalError(this ILogger logger, Exception exception);

    [LoggerMessage(
    EventId = 8047,
    Level = LogLevel.Debug,
    Message = "Getting all translation keys")]
    public static partial void LogGetTranslationKeysDebug(this ILogger logger);

    [LoggerMessage(
    EventId = 8048,
    Level = LogLevel.Error,
    Message = "Error retrieving translation keys")]
    public static partial void LogTranslationKeysRetrievalError(this ILogger logger, Exception exception);

    [LoggerMessage(
    EventId = 8049,
    Level = LogLevel.Debug,
    Message = "Getting translations for category: {Category}")]
    public static partial void LogGetTranslationsByCategoryDebug(this ILogger logger, string category);

    [LoggerMessage(
    EventId = 8050,
    Level = LogLevel.Error,
    Message = "Error retrieving translations for category: {Category}")]
    public static partial void LogTranslationsByCategoryRetrievalError(this ILogger logger, string category, Exception exception);

    // Employee endpoints logging (8061-8080)
    [LoggerMessage(
    EventId = 8061,
    Level = LogLevel.Information,
    Message = "User {UserId} not found in employee database, treating as Admin (likely service principal)")]
    public static partial void LogUserNotFoundTreatedAsAdmin(this ILogger logger, Guid userId);

    [LoggerMessage(
    EventId = 8062,
    Level = LogLevel.Information,
    Message = "No user ID in context, treating as Admin (likely service principal)")]
    public static partial void LogNoUserIdTreatedAsAdmin(this ILogger logger);

    [LoggerMessage(
    EventId = 8063,
    Level = LogLevel.Warning,
    Message = "SaveMyResponse failed: Unable to parse user ID from context")]
    public static partial void LogSaveResponseFailedNoUserId(this ILogger logger);

    [LoggerMessage(
    EventId = 8064,
    Level = LogLevel.Information,
    Message = "Received SaveMyResponse request for authenticated EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}")]
    public static partial void LogSaveResponseReceived(this ILogger logger, Guid employeeId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8065,
    Level = LogLevel.Warning,
    Message = "SaveMyResponse failed: Responses are null")]
    public static partial void LogSaveResponseFailedNullResponses(this ILogger logger);

    [LoggerMessage(
    EventId = 8066,
    Level = LogLevel.Information,
    Message = "SaveMyResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, ResponseId: {ResponseId}")]
    public static partial void LogSaveResponseCompleted(this ILogger logger, Guid employeeId, Guid assignmentId, Guid responseId);

    [LoggerMessage(
    EventId = 8067,
    Level = LogLevel.Warning,
    Message = "SaveMyResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}")]
    public static partial void LogSaveResponseFailed(this ILogger logger, Guid employeeId, Guid assignmentId, string errorMessage);

    [LoggerMessage(
    EventId = 8068,
    Level = LogLevel.Warning,
    Message = "SubmitMyResponse failed: Unable to parse user ID from context")]
    public static partial void LogSubmitResponseFailedNoUserId(this ILogger logger);

    [LoggerMessage(
    EventId = 8069,
    Level = LogLevel.Information,
    Message = "Received SubmitMyResponse request for authenticated EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}")]
    public static partial void LogSubmitResponseReceived(this ILogger logger, Guid employeeId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8070,
    Level = LogLevel.Information,
    Message = "SubmitMyResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}")]
    public static partial void LogSubmitResponseCompleted(this ILogger logger, Guid employeeId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8071,
    Level = LogLevel.Warning,
    Message = "SubmitMyResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}")]
    public static partial void LogSubmitResponseFailed(this ILogger logger, Guid employeeId, Guid assignmentId, string errorMessage);

    [LoggerMessage(
    EventId = 8072,
    Level = LogLevel.Information,
    Message = "Received ChangeEmployeeLanguage request for EmployeeId: {EmployeeId}, Language: {Language}")]
    public static partial void LogChangeLanguageReceived(this ILogger logger, Guid employeeId, string language);

    [LoggerMessage(
    EventId = 8073,
    Level = LogLevel.Warning,
    Message = "ChangeEmployeeLanguage failed: Unable to parse user ID from context")]
    public static partial void LogChangeLanguageFailedNoUserId(this ILogger logger);

    [LoggerMessage(
    EventId = 8074,
    Level = LogLevel.Warning,
    Message = "User {UserId} attempted to change language for employee {EmployeeId} without authorization")]
    public static partial void LogChangeLanguageUnauthorized(this ILogger logger, Guid userId, Guid employeeId);

    [LoggerMessage(
    EventId = 8075,
    Level = LogLevel.Information,
    Message = "ChangeEmployeeLanguage command result for EmployeeId {EmployeeId}: Success={Success}")]
    public static partial void LogChangeLanguageResult(this ILogger logger, Guid employeeId, bool success);

    [LoggerMessage(
    EventId = 8076,
    Level = LogLevel.Error,
    Message = "Error changing language for employee {EmployeeId}")]
    public static partial void LogChangeLanguageError(this ILogger logger, Guid employeeId, Exception exception);

    // Assignment endpoints logging (8081-8100)
    [LoggerMessage(
    EventId = 8081,
    Level = LogLevel.Information,
    Message = "Set AssignedBy to {AssignedBy} from user context {UserId}")]
    public static partial void LogSetAssignedByFromContext(this ILogger logger, Guid assignedBy, Guid userId);

    [LoggerMessage(
    EventId = 8082,
    Level = LogLevel.Error,
    Message = "Error creating bulk assignments")]
    public static partial void LogBulkAssignmentsCreationError(this ILogger logger, Exception exception);

    [LoggerMessage(
    EventId = 8083,
    Level = LogLevel.Information,
    Message = "Manager {ManagerId} attempting to create {Count} assignments")]
    public static partial void LogManagerBulkAssignmentsAttempt(this ILogger logger, Guid managerId, int count);

    [LoggerMessage(
    EventId = 8084,
    Level = LogLevel.Warning,
    Message = "CreateManagerBulkAssignments failed: {Message}")]
    public static partial void LogManagerBulkAssignmentsFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8085,
    Level = LogLevel.Warning,
    Message = "Manager {ManagerId} attempted to assign to employees who are not direct reports")]
    public static partial void LogManagerAssignmentUnauthorized(this ILogger logger, Guid managerId);

    [LoggerMessage(
    EventId = 8086,
    Level = LogLevel.Information,
    Message = "Set AssignedBy to {AssignedBy} for manager {ManagerId}")]
    public static partial void LogSetAssignedByForManager(this ILogger logger, Guid assignedBy, Guid managerId);

    // Employee query endpoints logging (8101-8150)
    [LoggerMessage(
    EventId = 8101,
    Level = LogLevel.Information,
    Message = "Received GetAllEmployees request - IncludeDeleted: {IncludeDeleted}, OrganizationNumber: {OrganizationNumber}, Role: {Role}, ManagerId: {ManagerId}")]
    public static partial void LogGetAllEmployeesRequest(this ILogger logger, bool includeDeleted, int? organizationNumber, string? role, Guid? managerId);

    [LoggerMessage(
    EventId = 8102,
    Level = LogLevel.Warning,
    Message = "GetAllEmployees authorization failed: {Message}")]
    public static partial void LogGetAllEmployeesAuthorizationFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8103,
    Level = LogLevel.Information,
    Message = "TeamLead {UserId} requesting employees - restricting to their direct reports")]
    public static partial void LogTeamLeadRequestingEmployees(this ILogger logger, Guid userId);

    [LoggerMessage(
    EventId = 8104,
    Level = LogLevel.Information,
    Message = "GetAllEmployees completed successfully, returned {EmployeeCount} employees")]
    public static partial void LogGetAllEmployeesCompleted(this ILogger logger, int employeeCount);

    [LoggerMessage(
    EventId = 8105,
    Level = LogLevel.Warning,
    Message = "GetAllEmployees failed: {ErrorMessage}")]
    public static partial void LogGetAllEmployeesFailed(this ILogger logger, string errorMessage);

    [LoggerMessage(
    EventId = 8106,
    Level = LogLevel.Error,
    Message = "Error retrieving employees")]
    public static partial void LogEmployeesRetrievalError(this ILogger logger, Exception exception);

    [LoggerMessage(
    EventId = 8107,
    Level = LogLevel.Information,
    Message = "Received GetEmployee request for EmployeeId: {EmployeeId}")]
    public static partial void LogGetEmployeeRequest(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8108,
    Level = LogLevel.Warning,
    Message = "GetEmployee authorization failed: {Message}")]
    public static partial void LogGetEmployeeAuthorizationFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8109,
    Level = LogLevel.Warning,
    Message = "User {UserId} attempted to access employee {EmployeeId} without authorization")]
    public static partial void LogEmployeeAccessUnauthorized(this ILogger logger, Guid userId, Guid employeeId);

    [LoggerMessage(
    EventId = 8110,
    Level = LogLevel.Information,
    Message = "Employee not found for EmployeeId: {EmployeeId}")]
    public static partial void LogEmployeeNotFound(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8111,
    Level = LogLevel.Information,
    Message = "GetEmployee completed successfully for EmployeeId: {EmployeeId}")]
    public static partial void LogGetEmployeeCompleted(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8112,
    Level = LogLevel.Error,
    Message = "Error retrieving employee {EmployeeId}")]
    public static partial void LogEmployeeRetrievalError(this ILogger logger, Guid employeeId, Exception exception);

    [LoggerMessage(
    EventId = 8141,
    Level = LogLevel.Warning,
    Message = "GetEmployee failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}")]
    public static partial void LogGetEmployeeFailed(this ILogger logger, Guid employeeId, string errorMessage);

    [LoggerMessage(
    EventId = 8113,
    Level = LogLevel.Information,
    Message = "Received GetDirectReports request for ManagerId: {ManagerId}")]
    public static partial void LogGetDirectReportsRequest(this ILogger logger, Guid managerId);

    [LoggerMessage(
    EventId = 8114,
    Level = LogLevel.Information,
    Message = "GetDirectReports completed successfully for ManagerId: {ManagerId}, returned {EmployeeCount} employees")]
    public static partial void LogGetDirectReportsCompleted(this ILogger logger, Guid managerId, int employeeCount);

    [LoggerMessage(
    EventId = 8115,
    Level = LogLevel.Warning,
    Message = "GetDirectReports failed: {ErrorMessage}")]
    public static partial void LogGetDirectReportsFailed(this ILogger logger, string errorMessage);

    [LoggerMessage(
    EventId = 8116,
    Level = LogLevel.Error,
    Message = "Error retrieving direct reports for manager {ManagerId}")]
    public static partial void LogDirectReportsRetrievalError(this ILogger logger, Guid managerId, Exception exception);

    [LoggerMessage(
    EventId = 8117,
    Level = LogLevel.Information,
    Message = "Received GetManagers request")]
    public static partial void LogGetManagersRequest(this ILogger logger);

    [LoggerMessage(
    EventId = 8118,
    Level = LogLevel.Information,
    Message = "GetManagers completed successfully, returned {ManagerCount} managers")]
    public static partial void LogGetManagersCompleted(this ILogger logger, int managerCount);

    [LoggerMessage(
    EventId = 8119,
    Level = LogLevel.Warning,
    Message = "GetManagers failed: {ErrorMessage}")]
    public static partial void LogGetManagersFailed(this ILogger logger, string errorMessage);

    [LoggerMessage(
    EventId = 8120,
    Level = LogLevel.Error,
    Message = "Error retrieving managers")]
    public static partial void LogManagersRetrievalError(this ILogger logger, Exception exception);

    [LoggerMessage(
    EventId = 8121,
    Level = LogLevel.Information,
    Message = "HasElevatedRole check - User {UserId} has role: {UserRole}")]
    public static partial void LogUserRoleCheck(this ILogger logger, Guid userId, string userRole);

    [LoggerMessage(
    EventId = 8122,
    Level = LogLevel.Warning,
    Message = "HasElevatedRole failed for user {UserId}: {ErrorMessage}")]
    public static partial void LogElevatedRoleCheckFailed(this ILogger logger, Guid userId, string errorMessage);

    [LoggerMessage(
    EventId = 8123,
    Level = LogLevel.Error,
    Message = "Error checking elevated role for user {UserId}")]
    public static partial void LogElevatedRoleCheckError(this ILogger logger, Guid userId, Exception exception);

    [LoggerMessage(
    EventId = 8124,
    Level = LogLevel.Information,
    Message = "Received GetEmployeeAssignments request for EmployeeId: {EmployeeId}")]
    public static partial void LogGetEmployeeAssignmentsRequest(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8125,
    Level = LogLevel.Information,
    Message = "GetEmployeeAssignments completed successfully for EmployeeId: {EmployeeId}, returned {AssignmentCount} assignments")]
    public static partial void LogGetEmployeeAssignmentsCompleted(this ILogger logger, Guid employeeId, int assignmentCount);

    [LoggerMessage(
    EventId = 8126,
    Level = LogLevel.Warning,
    Message = "GetEmployeeAssignments failed: {ErrorMessage}")]
    public static partial void LogGetEmployeeAssignmentsFailed(this ILogger logger, string errorMessage);

    [LoggerMessage(
    EventId = 8127,
    Level = LogLevel.Error,
    Message = "Error retrieving assignments for employee {EmployeeId}")]
    public static partial void LogEmployeeAssignmentsRetrievalError(this ILogger logger, Guid employeeId, Exception exception);

    [LoggerMessage(
    EventId = 8128,
    Level = LogLevel.Information,
    Message = "Received GetMyInfo request for EmployeeId: {EmployeeId}")]
    public static partial void LogGetMyInfoRequest(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8129,
    Level = LogLevel.Information,
    Message = "GetMyInfo completed successfully for EmployeeId: {EmployeeId}")]
    public static partial void LogGetMyInfoCompleted(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8130,
    Level = LogLevel.Warning,
    Message = "GetMyInfo failed: Employee not found for EmployeeId: {EmployeeId}")]
    public static partial void LogGetMyInfoEmployeeNotFound(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8131,
    Level = LogLevel.Warning,
    Message = "GetMyInfo failed: {ErrorMessage}")]
    public static partial void LogGetMyInfoFailed(this ILogger logger, string errorMessage);

    [LoggerMessage(
    EventId = 8132,
    Level = LogLevel.Error,
    Message = "Error retrieving my info for employee {EmployeeId}")]
    public static partial void LogMyInfoRetrievalError(this ILogger logger, Guid employeeId, Exception exception);

    [LoggerMessage(
    EventId = 8133,
    Level = LogLevel.Information,
    Message = "Received GetEmployeesByRole request for Role: {Role}")]
    public static partial void LogGetEmployeesByRoleRequest(this ILogger logger, string role);

    [LoggerMessage(
    EventId = 8134,
    Level = LogLevel.Information,
    Message = "GetEmployeesByRole completed successfully for Role: {Role}, returned {EmployeeCount} employees")]
    public static partial void LogGetEmployeesByRoleCompleted(this ILogger logger, string role, int employeeCount);

    [LoggerMessage(
    EventId = 8135,
    Level = LogLevel.Warning,
    Message = "GetEmployeesByRole failed: {ErrorMessage}")]
    public static partial void LogGetEmployeesByRoleFailed(this ILogger logger, string errorMessage);

    [LoggerMessage(
    EventId = 8136,
    Level = LogLevel.Error,
    Message = "Error retrieving employees by role {Role}")]
    public static partial void LogEmployeesByRoleRetrievalError(this ILogger logger, string role, Exception exception);

    [LoggerMessage(
    EventId = 8137,
    Level = LogLevel.Information,
    Message = "Received GetEmployeeMetrics request for EmployeeId: {EmployeeId}")]
    public static partial void LogGetEmployeeMetricsRequest(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8138,
    Level = LogLevel.Information,
    Message = "GetEmployeeMetrics completed successfully for EmployeeId: {EmployeeId}")]
    public static partial void LogGetEmployeeMetricsCompleted(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8139,
    Level = LogLevel.Warning,
    Message = "GetEmployeeMetrics failed: {ErrorMessage}")]
    public static partial void LogGetEmployeeMetricsFailed(this ILogger logger, string errorMessage);

    [LoggerMessage(
    EventId = 8140,
    Level = LogLevel.Error,
    Message = "Error retrieving metrics for employee {EmployeeId}")]
    public static partial void LogEmployeeMetricsRetrievalError(this ILogger logger, Guid employeeId, Exception exception);

    // Additional employee endpoints logging (8142-8160)
    [LoggerMessage(
    EventId = 8142,
    Level = LogLevel.Warning,
    Message = "GetMyAssignments failed: Unable to parse user ID from context")]
    public static partial void LogGetMyAssignmentsFailedNoUserId(this ILogger logger);

    [LoggerMessage(
    EventId = 8143,
    Level = LogLevel.Information,
    Message = "Received GetMyAssignments request for authenticated EmployeeId: {EmployeeId}, WorkflowState: {WorkflowState}")]
    public static partial void LogGetMyAssignmentsRequest(this ILogger logger, Guid employeeId, string? workflowState);

    [LoggerMessage(
    EventId = 8144,
    Level = LogLevel.Information,
    Message = "GetMyAssignments completed successfully for EmployeeId: {EmployeeId}, returned {AssignmentCount} assignments")]
    public static partial void LogGetMyAssignmentsCompleted(this ILogger logger, Guid employeeId, int assignmentCount);

    [LoggerMessage(
    EventId = 8145,
    Level = LogLevel.Warning,
    Message = "GetMyAssignments failed: {ErrorMessage}")]
    public static partial void LogGetMyAssignmentsFailed(this ILogger logger, string errorMessage);

    [LoggerMessage(
    EventId = 8146,
    Level = LogLevel.Error,
    Message = "Error retrieving my assignments for employee {EmployeeId}")]
    public static partial void LogMyAssignmentsRetrievalError(this ILogger logger, Guid employeeId, Exception exception);

    [LoggerMessage(
    EventId = 8147,
    Level = LogLevel.Warning,
    Message = "GetMyInfo failed: Unable to parse user ID from context")]
    public static partial void LogGetMyInfoFailedNoUserId(this ILogger logger);

    [LoggerMessage(
    EventId = 8148,
    Level = LogLevel.Information,
    Message = "Received GetEmployeesAssignedToMe request for ManagerId: {ManagerId}")]
    public static partial void LogGetEmployeesAssignedToMeRequest(this ILogger logger, Guid managerId);

    [LoggerMessage(
    EventId = 8149,
    Level = LogLevel.Information,
    Message = "GetEmployeesAssignedToMe completed successfully for ManagerId: {ManagerId}, returned {EmployeeCount} employees")]
    public static partial void LogGetEmployeesAssignedToMeCompleted(this ILogger logger, Guid managerId, int employeeCount);

    [LoggerMessage(
    EventId = 8150,
    Level = LogLevel.Warning,
    Message = "GetEmployeesAssignedToMe failed: {ErrorMessage}")]
    public static partial void LogGetEmployeesAssignedToMeFailed(this ILogger logger, string errorMessage);

    [LoggerMessage(
    EventId = 8151,
    Level = LogLevel.Error,
    Message = "Error retrieving employees assigned to manager {ManagerId}")]
    public static partial void LogEmployeesAssignedToMeRetrievalError(this ILogger logger, Guid managerId, Exception exception);

    [LoggerMessage(
    EventId = 8152,
    Level = LogLevel.Warning,
    Message = "GetEmployeesAssignedToMe failed: Unable to parse user ID from context")]
    public static partial void LogGetEmployeesAssignedToMeFailedNoUserId(this ILogger logger);

    // Additional pattern-specific logging (8160-8180)
    [LoggerMessage(
    EventId = 8160,
    Level = LogLevel.Warning,
    Message = "GetMyDashboard failed: Unable to parse user ID from context")]
    public static partial void LogGetMyDashboardFailedNoUserId(this ILogger logger);

    [LoggerMessage(
    EventId = 8161,
    Level = LogLevel.Information,
    Message = "Received GetMyDashboard request for authenticated EmployeeId: {EmployeeId}")]
    public static partial void LogGetMyDashboardRequest(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8162,
    Level = LogLevel.Warning,
    Message = "GetMyAssignmentById failed: Unable to parse user ID from context")]
    public static partial void LogGetMyAssignmentByIdFailedNoUserId(this ILogger logger);

    [LoggerMessage(
    EventId = 8163,
    Level = LogLevel.Warning,
    Message = "GetMyAssignmentCustomSections failed: Unable to parse user ID from context")]
    public static partial void LogGetMyAssignmentCustomSectionsFailedNoUserId(this ILogger logger);

    [LoggerMessage(
    EventId = 8164,
    Level = LogLevel.Information,
    Message = "Dashboard not found for EmployeeId: {EmployeeId} - this is expected for new employees with no assignments")]
    public static partial void LogDashboardNotFound(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8165,
    Level = LogLevel.Information,
    Message = "GetMyDashboard completed successfully for EmployeeId: {EmployeeId}")]
    public static partial void LogGetMyDashboardCompleted(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8166,
    Level = LogLevel.Warning,
    Message = "GetMyDashboard failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}")]
    public static partial void LogGetMyDashboardFailed(this ILogger logger, Guid employeeId, string errorMessage);

    [LoggerMessage(
    EventId = 8167,
    Level = LogLevel.Error,
    Message = "Error retrieving dashboard for authenticated employee {EmployeeId}")]
    public static partial void LogDashboardRetrievalError(this ILogger logger, Guid employeeId, Exception exception);

    // Assignment-specific endpoint logging (8170-8200)
    [LoggerMessage(
    EventId = 8170,
    Level = LogLevel.Information,
    Message = "Received GetMyAssignmentById request for authenticated EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}")]
    public static partial void LogGetMyAssignmentByIdRequest(this ILogger logger, Guid employeeId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8171,
    Level = LogLevel.Warning,
    Message = "GetMyAssignmentById failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}")]
    public static partial void LogGetMyAssignmentByIdFailed(this ILogger logger, Guid employeeId, string errorMessage);

    [LoggerMessage(
    EventId = 8172,
    Level = LogLevel.Warning,
    Message = "Assignment {AssignmentId} not found for EmployeeId: {EmployeeId}")]
    public static partial void LogAssignmentNotFoundForEmployee(this ILogger logger, Guid assignmentId, Guid employeeId);

    [LoggerMessage(
    EventId = 8173,
    Level = LogLevel.Information,
    Message = "GetMyAssignmentById completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}")]
    public static partial void LogGetMyAssignmentByIdCompleted(this ILogger logger, Guid employeeId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8174,
    Level = LogLevel.Error,
    Message = "Error retrieving assignment {AssignmentId} for authenticated employee {EmployeeId}")]
    public static partial void LogAssignmentRetrievalErrorForEmployee(this ILogger logger, Guid assignmentId, Guid employeeId, Exception exception);

    [LoggerMessage(
    EventId = 8175,
    Level = LogLevel.Information,
    Message = "Received GetMyAssignmentCustomSections request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}")]
    public static partial void LogGetMyAssignmentCustomSectionsRequest(this ILogger logger, Guid employeeId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8176,
    Level = LogLevel.Warning,
    Message = "GetMyAssignmentCustomSections failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}")]
    public static partial void LogGetMyAssignmentCustomSectionsFailed(this ILogger logger, Guid employeeId, string errorMessage);

    [LoggerMessage(
    EventId = 8177,
    Level = LogLevel.Information,
    Message = "GetMyAssignmentCustomSections completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}")]
    public static partial void LogGetMyAssignmentCustomSectionsCompleted(this ILogger logger, Guid employeeId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8178,
    Level = LogLevel.Error,
    Message = "Error retrieving custom sections for assignment {AssignmentId} for employee {EmployeeId}")]
    public static partial void LogCustomSectionsRetrievalErrorForEmployee(this ILogger logger, Guid assignmentId, Guid employeeId, Exception exception);

    [LoggerMessage(
    EventId = 8179,
    Level = LogLevel.Error,
    Message = "Error retrieving assignments for authenticated employee {EmployeeId}")]
    public static partial void LogAssignmentsRetrievalErrorForEmployee(this ILogger logger, Guid employeeId, Exception exception);

    // Response and goal-related endpoint logging (8180-8210)
    [LoggerMessage(
    EventId = 8180,
    Level = LogLevel.Warning,
    Message = "GetMyResponse failed: Unable to parse user ID from context")]
    public static partial void LogGetMyResponseFailedNoUserId(this ILogger logger);

    [LoggerMessage(
    EventId = 8181,
    Level = LogLevel.Information,
    Message = "Received GetMyResponse request for authenticated EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}")]
    public static partial void LogGetMyResponseRequest(this ILogger logger, Guid employeeId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8182,
    Level = LogLevel.Information,
    Message = "Response not found for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}")]
    public static partial void LogResponseNotFound(this ILogger logger, Guid employeeId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8183,
    Level = LogLevel.Warning,
    Message = "Employee {EmployeeId} attempted to access response for Assignment {AssignmentId} belonging to {ActualEmployeeId}")]
    public static partial void LogUnauthorizedResponseAccess(this ILogger logger, Guid employeeId, Guid assignmentId, Guid actualEmployeeId);

    [LoggerMessage(
    EventId = 8184,
    Level = LogLevel.Information,
    Message = "GetMyResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}")]
    public static partial void LogGetMyResponseCompleted(this ILogger logger, Guid employeeId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8185,
    Level = LogLevel.Error,
    Message = "Error retrieving response for assignment {AssignmentId} and employee {EmployeeId}")]
    public static partial void LogResponseRetrievalError(this ILogger logger, Guid assignmentId, Guid employeeId, Exception exception);

    [LoggerMessage(
    EventId = 8186,
    Level = LogLevel.Warning,
    Message = "Failed to parse user ID from context")]
    public static partial void LogFailedToParseUserId(this ILogger logger);

    [LoggerMessage(
    EventId = 8187,
    Level = LogLevel.Warning,
    Message = "Employee {UserId} attempted to access assignment {AssignmentId} that doesn't belong to them")]
    public static partial void LogUnauthorizedAssignmentAccess(this ILogger logger, Guid userId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8188,
    Level = LogLevel.Warning,
    Message = "Query returned null for assignment {AssignmentId}, question {QuestionId}")]
    public static partial void LogQueryReturnedNull(this ILogger logger, Guid assignmentId, Guid questionId);

    [LoggerMessage(
    EventId = 8189,
    Level = LogLevel.Information,
    Message = "Returning goal data for employee {UserId}: Assignment {AssignmentId}, Question {QuestionId}, WorkflowState: {WorkflowState}, Goals Count: {GoalCount}")]
    public static partial void LogReturningGoalData(this ILogger logger, Guid userId, Guid assignmentId, Guid questionId, string workflowState, int goalCount);

    [LoggerMessage(
    EventId = 8190,
    Level = LogLevel.Information,
    Message = "  Goal {GoalId}: AddedByRole={AddedByRole}")]
    public static partial void LogGoalDetail(this ILogger logger, Guid goalId, string addedByRole);


    // Final employee endpoint patterns (8200-8220)
    [LoggerMessage(
    EventId = 8200,
    Level = LogLevel.Warning,
    Message = "GetEmployeeAssignments authorization failed: {Message}")]
    public static partial void LogGetEmployeeAssignmentsAuthorizationFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8201,
    Level = LogLevel.Warning,
    Message = "Manager {UserId} attempted to access assignments for non-direct report employee {EmployeeId}")]
    public static partial void LogManagerAccessNonDirectReport(this ILogger logger, Guid userId, Guid employeeId);

    [LoggerMessage(
    EventId = 8202,
    Level = LogLevel.Warning,
    Message = "GetEmployeeAssignments failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}")]
    public static partial void LogGetEmployeeAssignmentsFailed(this ILogger logger, Guid employeeId, string errorMessage);


    [LoggerMessage(
    EventId = 8204,
    Level = LogLevel.Information,
    Message = "Received GetEmployeeLanguage request for EmployeeId: {EmployeeId}")]
    public static partial void LogGetEmployeeLanguageRequest(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8205,
    Level = LogLevel.Warning,
    Message = "GetEmployeeLanguage authorization failed: {Message}")]
    public static partial void LogGetEmployeeLanguageAuthorizationFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8206,
    Level = LogLevel.Warning,
    Message = "User {UserId} attempted to access language for employee {EmployeeId} without authorization")]
    public static partial void LogUnauthorizedEmployeeLanguageAccess(this ILogger logger, Guid userId, Guid employeeId);

    [LoggerMessage(
    EventId = 8207,
    Level = LogLevel.Warning,
    Message = "GetEmployeeLanguage failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}")]
    public static partial void LogGetEmployeeLanguageFailed(this ILogger logger, Guid employeeId, string errorMessage);

    [LoggerMessage(
    EventId = 8208,
    Level = LogLevel.Information,
    Message = "GetEmployeeLanguage completed successfully for EmployeeId: {EmployeeId}, Language: {Language}")]
    public static partial void LogGetEmployeeLanguageCompleted(this ILogger logger, Guid employeeId, string language);

    [LoggerMessage(
    EventId = 8209,
    Level = LogLevel.Error,
    Message = "Error retrieving language for employee {EmployeeId}")]
    public static partial void LogEmployeeLanguageRetrievalError(this ILogger logger, Guid employeeId, Exception exception);

    [LoggerMessage(
    EventId = 8210,
    Level = LogLevel.Warning,
    Message = "Unable to retrieve employee role for user {UserId}")]
    public static partial void LogUnableToRetrieveEmployeeRole(this ILogger logger, Guid userId);

    // Manager endpoints logging (8220-8260)
    [LoggerMessage(
    EventId = 8220,
    Level = LogLevel.Information,
    Message = "Received GetMyDashboard request for authenticated ManagerId: {ManagerId}")]
    public static partial void LogManagerDashboardRequest(this ILogger logger, Guid managerId);

    [LoggerMessage(
    EventId = 8221,
    Level = LogLevel.Warning,
    Message = "GetMyDashboard failed: {Message}")]
    public static partial void LogManagerDashboardFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8222,
    Level = LogLevel.Information,
    Message = "Dashboard not found for ManagerId: {ManagerId} - this is expected for new managers or managers with no team")]
    public static partial void LogManagerDashboardNotFound(this ILogger logger, Guid managerId);

    [LoggerMessage(
    EventId = 8223,
    Level = LogLevel.Information,
    Message = "GetMyDashboard completed successfully for ManagerId: {ManagerId}")]
    public static partial void LogManagerDashboardCompleted(this ILogger logger, Guid managerId);

    [LoggerMessage(
    EventId = 8224,
    Level = LogLevel.Warning,
    Message = "GetMyDashboard failed for ManagerId: {ManagerId}, Error: {ErrorMessage}")]
    public static partial void LogManagerDashboardError(this ILogger logger, Guid managerId, string errorMessage);

    [LoggerMessage(
    EventId = 8225,
    Level = LogLevel.Error,
    Message = "Error retrieving dashboard for manager {ManagerId}")]
    public static partial void LogManagerDashboardRetrievalError(this ILogger logger, Guid managerId, Exception exception);

    [LoggerMessage(
    EventId = 8226,
    Level = LogLevel.Information,
    Message = "Received GetMyTeamMembers request for authenticated ManagerId: {ManagerId}")]
    public static partial void LogTeamMembersRequest(this ILogger logger, Guid managerId);

    [LoggerMessage(
    EventId = 8227,
    Level = LogLevel.Warning,
    Message = "GetMyTeamMembers failed: {Message}")]
    public static partial void LogTeamMembersFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8228,
    Level = LogLevel.Information,
    Message = "GetTeamMembers completed successfully for ManagerId: {ManagerId}, returned {TeamCount} members")]
    public static partial void LogTeamMembersCompleted(this ILogger logger, Guid managerId, int teamCount);

    [LoggerMessage(
    EventId = 8229,
    Level = LogLevel.Warning,
    Message = "GetTeamMembers failed for ManagerId: {ManagerId}, Error: {ErrorMessage}")]
    public static partial void LogTeamMembersError(this ILogger logger, Guid managerId, string errorMessage);

    [LoggerMessage(
    EventId = 8230,
    Level = LogLevel.Error,
    Message = "Error retrieving team members for manager {ManagerId}")]
    public static partial void LogTeamMembersRetrievalError(this ILogger logger, Guid managerId, Exception exception);

    [LoggerMessage(
    EventId = 8231,
    Level = LogLevel.Information,
    Message = "Received GetMyTeamAssignments request for authenticated ManagerId: {ManagerId}, WorkflowState: {WorkflowState}")]
    public static partial void LogTeamAssignmentsRequest(this ILogger logger, Guid managerId, string? workflowState);

    [LoggerMessage(
    EventId = 8232,
    Level = LogLevel.Warning,
    Message = "GetMyTeamAssignments failed: {Message}")]
    public static partial void LogTeamAssignmentsFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8233,
    Level = LogLevel.Information,
    Message = "GetTeamAssignments completed successfully for ManagerId: {ManagerId}, returned {AssignmentCount} assignments")]
    public static partial void LogTeamAssignmentsCompleted(this ILogger logger, Guid managerId, int assignmentCount);

    [LoggerMessage(
    EventId = 8234,
    Level = LogLevel.Warning,
    Message = "GetTeamAssignments failed for ManagerId: {ManagerId}, Error: {ErrorMessage}")]
    public static partial void LogTeamAssignmentsError(this ILogger logger, Guid managerId, string errorMessage);

    [LoggerMessage(
    EventId = 8235,
    Level = LogLevel.Error,
    Message = "Error retrieving team assignments for manager {ManagerId}")]
    public static partial void LogTeamAssignmentsRetrievalError(this ILogger logger, Guid managerId, Exception exception);

    [LoggerMessage(
    EventId = 8236,
    Level = LogLevel.Information,
    Message = "Received GetMyTeamProgress request for authenticated ManagerId: {ManagerId}")]
    public static partial void LogTeamProgressRequest(this ILogger logger, Guid managerId);

    [LoggerMessage(
    EventId = 8237,
    Level = LogLevel.Warning,
    Message = "GetMyTeamProgress failed: {Message}")]
    public static partial void LogTeamProgressFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8238,
    Level = LogLevel.Information,
    Message = "GetTeamProgress completed successfully for ManagerId: {ManagerId}, returned {ProgressCount} items")]
    public static partial void LogTeamProgressCompleted(this ILogger logger, Guid managerId, int progressCount);

    [LoggerMessage(
    EventId = 8239,
    Level = LogLevel.Warning,
    Message = "GetTeamProgress failed for ManagerId: {ManagerId}, Error: {ErrorMessage}")]
    public static partial void LogTeamProgressError(this ILogger logger, Guid managerId, string errorMessage);

    [LoggerMessage(
    EventId = 8240,
    Level = LogLevel.Error,
    Message = "Error retrieving team progress for manager {ManagerId}")]
    public static partial void LogTeamProgressRetrievalError(this ILogger logger, Guid managerId, Exception exception);

    [LoggerMessage(
    EventId = 8241,
    Level = LogLevel.Information,
    Message = "Received GetMyTeamAnalytics request for authenticated ManagerId: {ManagerId}")]
    public static partial void LogTeamAnalyticsRequest(this ILogger logger, Guid managerId);

    [LoggerMessage(
    EventId = 8242,
    Level = LogLevel.Warning,
    Message = "GetMyTeamAnalytics failed: {Message}")]
    public static partial void LogTeamAnalyticsFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8243,
    Level = LogLevel.Information,
    Message = "GetTeamAnalytics completed successfully for ManagerId: {ManagerId}")]
    public static partial void LogTeamAnalyticsCompleted(this ILogger logger, Guid managerId);

    [LoggerMessage(
    EventId = 8244,
    Level = LogLevel.Warning,
    Message = "GetTeamAnalytics failed for ManagerId: {ManagerId}, Error: {ErrorMessage}")]
    public static partial void LogTeamAnalyticsError(this ILogger logger, Guid managerId, string errorMessage);

    [LoggerMessage(
    EventId = 8245,
    Level = LogLevel.Error,
    Message = "Error retrieving team analytics for manager {ManagerId}")]
    public static partial void LogTeamAnalyticsRetrievalError(this ILogger logger, Guid managerId, Exception exception);

    [LoggerMessage(
    EventId = 8246,
    Level = LogLevel.Warning,
    Message = "GetManagerTeamMembers failed: {Message}")]
    public static partial void LogManagerTeamMembersFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8247,
    Level = LogLevel.Warning,
    Message = "User {RequestingUserId} not authorized to view manager {ManagerId} team")]
    public static partial void LogManagerTeamMembersUnauthorized(this ILogger logger, Guid requestingUserId, Guid managerId);

    [LoggerMessage(
    EventId = 8248,
    Level = LogLevel.Information,
    Message = "User {RequestingUserId} viewing team for ManagerId: {ManagerId}")]
    public static partial void LogManagerTeamMembersViewing(this ILogger logger, Guid requestingUserId, Guid managerId);

    [LoggerMessage(
    EventId = 8249,
    Level = LogLevel.Warning,
    Message = "GetManagerTeamAssignments failed: {Message}")]
    public static partial void LogManagerTeamAssignmentsFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8250,
    Level = LogLevel.Warning,
    Message = "User {RequestingUserId} not authorized to view manager {ManagerId} team assignments")]
    public static partial void LogManagerTeamAssignmentsUnauthorized(this ILogger logger, Guid requestingUserId, Guid managerId);

    [LoggerMessage(
    EventId = 8251,
    Level = LogLevel.Information,
    Message = "User {RequestingUserId} viewing team assignments for ManagerId: {ManagerId}, WorkflowState: {WorkflowState}")]
    public static partial void LogManagerTeamAssignmentsViewing(this ILogger logger, Guid requestingUserId, Guid managerId, string? workflowState);

    [LoggerMessage(
    EventId = 8252,
    Level = LogLevel.Error,
    Message = "Error retrieving assignments")]
    public static partial void LogAssignmentsRetrievalError(this ILogger logger, Exception exception);

    [LoggerMessage(
    EventId = 8253,
    Level = LogLevel.Warning,
    Message = "GetAssignment authorization failed: {Message}")]
    public static partial void LogAssignmentAuthorizationFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8254,
    Level = LogLevel.Warning,
    Message = "Manager {UserId} attempted to access assignment {AssignmentId} for non-direct report")]
    public static partial void LogAssignmentAccessViolation(this ILogger logger, Guid userId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8255,
    Level = LogLevel.Error,
    Message = "Error retrieving assignment {AssignmentId}")]
    public static partial void LogAssignmentRetrievalError(this ILogger logger, Guid assignmentId, Exception exception);

    [LoggerMessage(
    EventId = 8256,
    Level = LogLevel.Warning,
    Message = "GetAssignmentsByEmployee authorization failed: {Message}")]
    public static partial void LogAssignmentsByEmployeeAuthorizationFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8257,
    Level = LogLevel.Warning,
    Message = "Manager {UserId} attempted to access assignments for non-direct report employee {EmployeeId}")]
    public static partial void LogAssignmentsByEmployeeAccessViolation(this ILogger logger, Guid userId, Guid employeeId);

    [LoggerMessage(
    EventId = 8258,
    Level = LogLevel.Error,
    Message = "Error retrieving assignments for employee {EmployeeId}")]
    public static partial void LogAssignmentsByEmployeeRetrievalError(this ILogger logger, Guid employeeId, Exception exception);

    [LoggerMessage(
    EventId = 8259,
    Level = LogLevel.Warning,
    Message = "GetReviewChanges authorization failed: {Message}")]
    public static partial void LogReviewChangesAuthorizationFailed(this ILogger logger, string message);

    [LoggerMessage(
    EventId = 8260,
    Level = LogLevel.Warning,
    Message = "Manager {UserId} attempted to access review changes for assignment {AssignmentId} for non-direct report")]
    public static partial void LogReviewChangesAccessViolation(this ILogger logger, Guid userId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8261,
    Level = LogLevel.Error,
    Message = "Error retrieving review changes for assignment {AssignmentId}")]
    public static partial void LogReviewChangesRetrievalError(this ILogger logger, Guid assignmentId, Exception exception);

    [LoggerMessage(
    EventId = 8262,
    Level = LogLevel.Warning,
    Message = "Failed to parse user ID from context")]
    public static partial void LogUserIdParsingFailed(this ILogger logger);

    [LoggerMessage(
    EventId = 8263,
    Level = LogLevel.Warning,
    Message = "Manager {UserId} attempted to access custom sections for assignment {AssignmentId} for non-direct report")]
    public static partial void LogCustomSectionsAccessViolation(this ILogger logger, Guid userId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8264,
    Level = LogLevel.Error,
    Message = "Error retrieving custom sections for assignment {AssignmentId}")]
    public static partial void LogCustomSectionsRetrievalError(this ILogger logger, Guid assignmentId, Exception exception);

    [LoggerMessage(
    EventId = 8265,
    Level = LogLevel.Warning,
    Message = "Assignment {AssignmentId} not found")]
    public static partial void LogAssignmentNotFound(this ILogger logger, Guid assignmentId);

    [LoggerMessage(
    EventId = 8266,
    Level = LogLevel.Warning,
    Message = "Manager {UserId} attempted to access predecessors for assignment {AssignmentId} for non-direct report")]
    public static partial void LogPredecessorsAccessViolation(this ILogger logger, Guid userId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8267,
    Level = LogLevel.Warning,
    Message = "Query returned null for assignment {AssignmentId}")]
    public static partial void LogAssignmentQueryReturnedNull(this ILogger logger, Guid assignmentId);

    [LoggerMessage(
    EventId = 8268,
    Level = LogLevel.Error,
    Message = "Error getting available predecessors for assignment {AssignmentId}")]
    public static partial void LogPredecessorsRetrievalError(this ILogger logger, Guid assignmentId, Exception exception);

    [LoggerMessage(
    EventId = 8269,
    Level = LogLevel.Warning,
    Message = "Unable to retrieve employee role for user {UserId}")]
    public static partial void LogEmployeeRoleRetrievalFailed(this ILogger logger, Guid userId);

    [LoggerMessage(
    EventId = 8270,
    Level = LogLevel.Warning,
    Message = "Query returned null for assignment {AssignmentId}, question {QuestionId}")]
    public static partial void LogAssignmentQuestionQueryReturnedNull(this ILogger logger, Guid assignmentId, Guid questionId);

    [LoggerMessage(
    EventId = 8271,
    Level = LogLevel.Error,
    Message = "Error getting goal data for assignment {AssignmentId}, question {QuestionId}")]
    public static partial void LogGoalDataRetrievalError(this ILogger logger, Guid assignmentId, Guid questionId, Exception exception);

    [LoggerMessage(
    EventId = 8272,
    Level = LogLevel.Error,
    Message = "Error getting available feedback for assignment {AssignmentId}")]
    public static partial void LogFeedbackRetrievalError(this ILogger logger, Guid assignmentId, Exception exception);

    [LoggerMessage(
    EventId = 8273,
    Level = LogLevel.Error,
    Message = "Error getting feedback data for assignment {AssignmentId}, question {QuestionId}")]
    public static partial void LogFeedbackDataRetrievalError(this ILogger logger, Guid assignmentId, Guid questionId, Exception exception);

    [LoggerMessage(
    EventId = 8274,
    Level = LogLevel.Error,
    Message = "Error retrieving responses")]
    public static partial void LogResponsesRetrievalError(this ILogger logger, Exception exception);

    [LoggerMessage(
    EventId = 8275,
    Level = LogLevel.Error,
    Message = "Error retrieving response {ResponseId}")]
    public static partial void LogResponseRetrievalError(this ILogger logger, Guid responseId, Exception exception);

    [LoggerMessage(
    EventId = 8276,
    Level = LogLevel.Warning,
    Message = "GetResponseByAssignment: Unable to parse user ID from context")]
    public static partial void LogResponseByAssignmentUserIdParsingFailed(this ILogger logger);

    [LoggerMessage(
    EventId = 8277,
    Level = LogLevel.Warning,
    Message = "GetResponseByAssignment: User role not found for user {UserId}")]
    public static partial void LogResponseByAssignmentUserRoleNotFound(this ILogger logger, Guid userId);

    [LoggerMessage(
    EventId = 8278,
    Level = LogLevel.Warning,
    Message = "GetResponseByAssignment: Template not found for assignment {AssignmentId}")]
    public static partial void LogResponseByAssignmentTemplateNotFound(this ILogger logger, Guid assignmentId);

    [LoggerMessage(
    EventId = 8279,
    Level = LogLevel.Error,
    Message = "Error retrieving response for assignment {AssignmentId}")]
    public static partial void LogResponseByAssignmentRetrievalError(this ILogger logger, Guid assignmentId, Exception exception);

    [LoggerMessage(
    EventId = 8280,
    Level = LogLevel.Information,
    Message = "Received GetEmployeeAssignments request for EmployeeId: {EmployeeId}")]
    public static partial void LogEmployeeAssignmentsRequest(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8281,
    Level = LogLevel.Information,
    Message = "GetEmployeeAssignments completed successfully for EmployeeId: {EmployeeId}, returned {Count} assignments")]
    public static partial void LogEmployeeAssignmentsCompleted(this ILogger logger, Guid employeeId, int count);

    [LoggerMessage(
    EventId = 8282,
    Level = LogLevel.Warning,
    Message = "GetEmployeeAssignments failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}")]
    public static partial void LogEmployeeAssignmentsError(this ILogger logger, Guid employeeId, string errorMessage);


    [LoggerMessage(
    EventId = 8284,
    Level = LogLevel.Information,
    Message = "Received GetEmployeeResponse request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}")]
    public static partial void LogEmployeeResponseRequest(this ILogger logger, Guid employeeId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8285,
    Level = LogLevel.Information,
    Message = "Response not found for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}")]
    public static partial void LogEmployeeResponseNotFound(this ILogger logger, Guid employeeId, Guid assignmentId);

    [LoggerMessage(
    EventId = 8286,
    Level = LogLevel.Warning,
    Message = "Employee {EmployeeId} attempted to access response for Assignment {AssignmentId} belonging to {ActualEmployeeId}")]
    public static partial void LogEmployeeResponseAccessViolation(this ILogger logger, Guid employeeId, Guid assignmentId, Guid actualEmployeeId);

    [LoggerMessage(
    EventId = 8287,
    Level = LogLevel.Warning,
    Message = "Failed to calculate progress for assignment {AssignmentId}, defaulting to 0")]
    public static partial void LogProgressCalculationFailed(this ILogger logger, Exception exception, Guid assignmentId);

    [LoggerMessage(
    EventId = 8288,
    Level = LogLevel.Error,
    Message = "Error retrieving response for assignment {AssignmentId} and employee {EmployeeId}")]
    public static partial void LogEmployeeResponseRetrievalError(this ILogger logger, Guid assignmentId, Guid employeeId, Exception exception);

    [LoggerMessage(
    EventId = 8289,
    Level = LogLevel.Information,
    Message = "Received GetEmployeeAssignmentProgress request for EmployeeId: {EmployeeId}")]
    public static partial void LogEmployeeAssignmentProgressRequest(this ILogger logger, Guid employeeId);

    [LoggerMessage(
    EventId = 8290,
    Level = LogLevel.Error,
    Message = "Error retrieving assignment progress for employee {EmployeeId}")]
    public static partial void LogEmployeeAssignmentProgressRetrievalError(this ILogger logger, Guid employeeId, Exception exception);

    // Manager Response Logging - EventId 8291-8296
    [LoggerMessage(
        EventId = 8291,
        Level = LogLevel.Warning,
        Message = "SaveManagerResponse failed: Unable to parse user ID from context")]
    public static partial void LogSaveManagerResponseFailedNoUserId(this ILogger logger);

    [LoggerMessage(
        EventId = 8292,
        Level = LogLevel.Information,
        Message = "Received SaveManagerResponse request for ManagerId: {ManagerId}, AssignmentId: {AssignmentId}")]
    public static partial void LogSaveManagerResponseReceived(this ILogger logger, Guid managerId, Guid assignmentId);

    [LoggerMessage(
        EventId = 8293,
        Level = LogLevel.Warning,
        Message = "Manager {ManagerId} attempted to save response for assignment {AssignmentId} without authorization")]
    public static partial void LogSaveManagerResponseUnauthorized(this ILogger logger, Guid managerId, Guid assignmentId);

    [LoggerMessage(
        EventId = 8294,
        Level = LogLevel.Warning,
        Message = "SaveManagerResponse failed: Responses are null")]
    public static partial void LogSaveManagerResponseFailedNullResponses(this ILogger logger);

    [LoggerMessage(
        EventId = 8295,
        Level = LogLevel.Information,
        Message = "SaveManagerResponse completed successfully for ManagerId: {ManagerId}, AssignmentId: {AssignmentId}, ResponseId: {ResponseId}")]
    public static partial void LogSaveManagerResponseCompleted(this ILogger logger, Guid managerId, Guid assignmentId, Guid responseId);

    [LoggerMessage(
        EventId = 8296,
        Level = LogLevel.Warning,
        Message = "SaveManagerResponse failed for ManagerId: {ManagerId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}")]
    public static partial void LogSaveManagerResponseFailed(this ILogger logger, Guid managerId, Guid assignmentId, string errorMessage);

    // Organization Endpoints Logging - EventId 8297-8310
    [LoggerMessage(
        EventId = 8297,
        Level = LogLevel.Information,
        Message = "Received GetAllOrganizations request - IncludeDeleted: {IncludeDeleted}, IncludeIgnored: {IncludeIgnored}, ParentId: {ParentId}, ManagerId: {ManagerId}")]
    public static partial void LogGetAllOrganizationsRequest(this ILogger logger, bool includeDeleted, bool includeIgnored, Guid? parentId, string? managerId);

    [LoggerMessage(
        EventId = 8298,
        Level = LogLevel.Information,
        Message = "Successfully returned {Count} organizations")]
    public static partial void LogGetAllOrganizationsSuccess(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 8299,
        Level = LogLevel.Warning,
        Message = "Failed to retrieve organizations: {ErrorMessage}")]
    public static partial void LogGetAllOrganizationsFailed(this ILogger logger, string errorMessage);

    [LoggerMessage(
        EventId = 8300,
        Level = LogLevel.Error,
        Message = "Unexpected error occurred while processing GetAllOrganizations request")]
    public static partial void LogGetAllOrganizationsError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 8301,
        Level = LogLevel.Information,
        Message = "Received GetOrganizationById request for Id: {Id}")]
    public static partial void LogGetOrganizationByIdRequest(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 8302,
        Level = LogLevel.Information,
        Message = "Organization with Id {Id} not found")]
    public static partial void LogOrganizationByIdNotFound(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 8303,
        Level = LogLevel.Information,
        Message = "Successfully returned organization with Id: {Id}")]
    public static partial void LogGetOrganizationByIdSuccess(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 8304,
        Level = LogLevel.Warning,
        Message = "Failed to retrieve organization with Id {Id}: {ErrorMessage}")]
    public static partial void LogGetOrganizationByIdFailed(this ILogger logger, Guid id, string errorMessage);

    [LoggerMessage(
        EventId = 8305,
        Level = LogLevel.Error,
        Message = "Unexpected error occurred while processing GetOrganizationById request for Id: {Id}")]
    public static partial void LogGetOrganizationByIdError(this ILogger logger, Exception exception, Guid id);

    [LoggerMessage(
        EventId = 8306,
        Level = LogLevel.Information,
        Message = "Received GetOrganizationByNumber request for Number: {Number}")]
    public static partial void LogGetOrganizationByNumberRequest(this ILogger logger, string number);

    [LoggerMessage(
        EventId = 8307,
        Level = LogLevel.Information,
        Message = "Organization with Number {Number} not found")]
    public static partial void LogOrganizationByNumberNotFound(this ILogger logger, string number);

    [LoggerMessage(
        EventId = 8308,
        Level = LogLevel.Information,
        Message = "Successfully returned organization with Number: {Number}")]
    public static partial void LogGetOrganizationByNumberSuccess(this ILogger logger, string number);

    [LoggerMessage(
        EventId = 8309,
        Level = LogLevel.Warning,
        Message = "Failed to retrieve organization with Number {Number}: {ErrorMessage}")]
    public static partial void LogGetOrganizationByNumberFailed(this ILogger logger, string number, string errorMessage);

    [LoggerMessage(
        EventId = 8310,
        Level = LogLevel.Error,
        Message = "Unexpected error occurred while processing GetOrganizationByNumber request for Number: {Number}")]
    public static partial void LogGetOrganizationByNumberError(this ILogger logger, Exception exception, string number);

    // Translation Endpoints Logging (CommandApi) - EventId 8311-8323
    [LoggerMessage(
        EventId = 8311,
        Level = LogLevel.Information,
        Message = "Upserting translation for key: {Key}")]
    public static partial void LogUpsertTranslationRequest(this ILogger logger, string key);

    [LoggerMessage(
        EventId = 8312,
        Level = LogLevel.Information,
        Message = "Successfully upserted translation for key: {Key}")]
    public static partial void LogUpsertTranslationSuccess(this ILogger logger, string key);

    [LoggerMessage(
        EventId = 8313,
        Level = LogLevel.Error,
        Message = "Error upserting translation for key: {Key}")]
    public static partial void LogUpsertTranslationError(this ILogger logger, Exception exception, string key);

    [LoggerMessage(
        EventId = 8314,
        Level = LogLevel.Information,
        Message = "Deleting translation for key: {Key}")]
    public static partial void LogDeleteTranslationRequest(this ILogger logger, string key);

    [LoggerMessage(
        EventId = 8315,
        Level = LogLevel.Information,
        Message = "Successfully deleted translation for key: {Key}")]
    public static partial void LogDeleteTranslationSuccess(this ILogger logger, string key);

    [LoggerMessage(
        EventId = 8316,
        Level = LogLevel.Warning,
        Message = "Translation not found for deletion: {Key}")]
    public static partial void LogDeleteTranslationNotFound(this ILogger logger, string key);

    [LoggerMessage(
        EventId = 8317,
        Level = LogLevel.Error,
        Message = "Error deleting translation for key: {Key}")]
    public static partial void LogDeleteTranslationError(this ILogger logger, Exception exception, string key);

    [LoggerMessage(
        EventId = 8318,
        Level = LogLevel.Information,
        Message = "Bulk importing {Count} translations requested by admin")]
    public static partial void LogBulkImportTranslationsRequest(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 8319,
        Level = LogLevel.Information,
        Message = "Successfully bulk imported {Count} translations")]
    public static partial void LogBulkImportTranslationsSuccess(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 8320,
        Level = LogLevel.Error,
        Message = "Error bulk importing translations")]
    public static partial void LogBulkImportTranslationsError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 8321,
        Level = LogLevel.Information,
        Message = "Translation cache invalidation requested by admin")]
    public static partial void LogInvalidateCacheRequest(this ILogger logger);

    [LoggerMessage(
        EventId = 8322,
        Level = LogLevel.Information,
        Message = "Successfully invalidated translation cache")]
    public static partial void LogInvalidateCacheSuccess(this ILogger logger);

    [LoggerMessage(
        EventId = 8323,
        Level = LogLevel.Error,
        Message = "Error invalidating translation cache")]
    public static partial void LogInvalidateCacheError(this ILogger logger, Exception exception);

    // Assignment Endpoints Additional Logging - EventId 8324-8327
    [LoggerMessage(
        EventId = 8324,
        Level = LogLevel.Warning,
        Message = "Manager {ManagerId} attempted to assign to employees who are not direct reports")]
    public static partial void LogManagerAssignNonDirectReports(this ILogger logger, Guid managerId);

    [LoggerMessage(
        EventId = 8325,
        Level = LogLevel.Information,
        Message = "Manager {ManagerId} successfully created {Count} assignments")]
    public static partial void LogManagerCreateAssignmentsSuccess(this ILogger logger, Guid managerId, int count);

    [LoggerMessage(
        EventId = 8326,
        Level = LogLevel.Error,
        Message = "Error starting assignment work")]
    public static partial void LogStartAssignmentWorkError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 8327,
        Level = LogLevel.Error,
        Message = "Error completing assignment work")]
    public static partial void LogCompleteAssignmentWorkError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 8328,
        Level = LogLevel.Error,
        Message = "Error creating manager bulk assignments")]
    public static partial void LogCreateManagerBulkAssignmentsError(this ILogger logger, Exception exception);

    // Replay Endpoints Logging - EventId 8329-8338
    [LoggerMessage(
        EventId = 8329,
        Level = LogLevel.Information,
        Message = "Retrieving replay status for replay {ReplayId}")]
    public static partial void LogRetrieveReplayStatusRequest(this ILogger logger, Guid replayId);

    [LoggerMessage(
        EventId = 8330,
        Level = LogLevel.Information,
        Message = "Successfully retrieved replay status for replay {ReplayId}")]
    public static partial void LogRetrieveReplayStatusSuccess(this ILogger logger, Guid replayId);

    [LoggerMessage(
        EventId = 8331,
        Level = LogLevel.Warning,
        Message = "Replay {ReplayId} not found")]
    public static partial void LogReplayNotFound(this ILogger logger, Guid replayId);

    [LoggerMessage(
        EventId = 8332,
        Level = LogLevel.Error,
        Message = "Error retrieving replay status for replay {ReplayId}")]
    public static partial void LogRetrieveReplayStatusError(this ILogger logger, Exception exception, Guid replayId);

    [LoggerMessage(
        EventId = 8333,
        Level = LogLevel.Information,
        Message = "Retrieving replay history with limit {Limit}")]
    public static partial void LogRetrieveReplayHistoryRequest(this ILogger logger, int limit);

    [LoggerMessage(
        EventId = 8334,
        Level = LogLevel.Information,
        Message = "Successfully retrieved {Count} replay history entries")]
    public static partial void LogRetrieveReplayHistorySuccess(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 8335,
        Level = LogLevel.Error,
        Message = "Error retrieving replay history")]
    public static partial void LogRetrieveReplayHistoryError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 8336,
        Level = LogLevel.Information,
        Message = "Retrieving available projections (rebuildableOnly: {RebuildableOnly})")]
    public static partial void LogRetrieveProjectionsRequest(this ILogger logger, bool rebuildableOnly);

    [LoggerMessage(
        EventId = 8337,
        Level = LogLevel.Information,
        Message = "Successfully retrieved {Count} available projections")]
    public static partial void LogRetrieveProjectionsSuccess(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 8338,
        Level = LogLevel.Error,
        Message = "Error retrieving available projections")]
    public static partial void LogRetrieveProjectionsError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 8339,
        Level = LogLevel.Information,
        Message = "Set AssignedBy to {AssignedByName} from user context {UserId}")]
    public static partial void LogSetAssignedByFromUserContext(this ILogger logger, string assignedByName, Guid userId);

    [LoggerMessage(
        EventId = 8340,
        Level = LogLevel.Information,
        Message = "Set AssignedBy to {AssignedByName} for manager {ManagerId}")]
    public static partial void LogSetAssignedByForManagerName(this ILogger logger, string assignedByName, Guid managerId);

    // Template logging methods (EventIds 8341-8349)
    [LoggerMessage(
        EventId = 8341,
        Level = LogLevel.Error,
        Message = "Error creating questionnaire template")]
    public static partial void LogCreateTemplateError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 8342,
        Level = LogLevel.Error,
        Message = "Error updating template {TemplateId}")]
    public static partial void LogUpdateTemplateError(this ILogger logger, Exception exception, Guid templateId);

    [LoggerMessage(
        EventId = 8343,
        Level = LogLevel.Error,
        Message = "Error deleting template {TemplateId}")]
    public static partial void LogDeleteTemplateError(this ILogger logger, Exception exception, Guid templateId);

    [LoggerMessage(
        EventId = 8344,
        Level = LogLevel.Warning,
        Message = "Cannot publish template {TemplateId}: unable to parse user ID from context")]
    public static partial void LogPublishTemplateInvalidUserId(this ILogger logger, Guid templateId);

    [LoggerMessage(
        EventId = 8345,
        Level = LogLevel.Error,
        Message = "Error publishing template {TemplateId}")]
    public static partial void LogPublishTemplateError(this ILogger logger, Exception exception, Guid templateId);

    [LoggerMessage(
        EventId = 8346,
        Level = LogLevel.Error,
        Message = "Error unpublishing template {TemplateId}")]
    public static partial void LogUnpublishTemplateError(this ILogger logger, Exception exception, Guid templateId);

    [LoggerMessage(
        EventId = 8347,
        Level = LogLevel.Error,
        Message = "Error archiving template {TemplateId}")]
    public static partial void LogArchiveTemplateError(this ILogger logger, Exception exception, Guid templateId);

    [LoggerMessage(
        EventId = 8348,
        Level = LogLevel.Error,
        Message = "Error restoring template {TemplateId}")]
    public static partial void LogRestoreTemplateError(this ILogger logger, Exception exception, Guid templateId);

    [LoggerMessage(
        EventId = 8349,
        Level = LogLevel.Error,
        Message = "Error cloning template {TemplateId}")]
    public static partial void LogCloneTemplateError(this ILogger logger, Exception exception, Guid templateId);

    // Template query logging methods (EventIds 8350-8355)
    [LoggerMessage(
        EventId = 8350,
        Level = LogLevel.Error,
        Message = "Error retrieving questionnaire templates")]
    public static partial void LogRetrieveTemplatesError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 8351,
        Level = LogLevel.Error,
        Message = "Error retrieving template {TemplateId}")]
    public static partial void LogRetrieveTemplateError(this ILogger logger, Exception exception, Guid templateId);

    [LoggerMessage(
        EventId = 8352,
        Level = LogLevel.Error,
        Message = "Error retrieving published questionnaire templates")]
    public static partial void LogRetrievePublishedTemplatesError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 8353,
        Level = LogLevel.Error,
        Message = "Error retrieving draft questionnaire templates")]
    public static partial void LogRetrieveDraftTemplatesError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 8354,
        Level = LogLevel.Error,
        Message = "Error retrieving archived questionnaire templates")]
    public static partial void LogRetrieveArchivedTemplatesError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 8355,
        Level = LogLevel.Error,
        Message = "Error retrieving assignable questionnaire templates")]
    public static partial void LogRetrieveAssignableTemplatesError(this ILogger logger, Exception exception);

    // HR dashboard logging methods (EventIds 8356-8360)
    [LoggerMessage(
        EventId = 8356,
        Level = LogLevel.Information,
        Message = "Received GetHRDashboard request")]
    public static partial void LogGetHRDashboardRequest(this ILogger logger);

    [LoggerMessage(
        EventId = 8357,
        Level = LogLevel.Information,
        Message = "HR dashboard not found - this is expected for new systems")]
    public static partial void LogHRDashboardNotFound(this ILogger logger);

    [LoggerMessage(
        EventId = 8358,
        Level = LogLevel.Information,
        Message = "GetHRDashboard completed successfully")]
    public static partial void LogGetHRDashboardSuccess(this ILogger logger);

    [LoggerMessage(
        EventId = 8359,
        Level = LogLevel.Warning,
        Message = "GetHRDashboard failed, Error: {ErrorMessage}")]
    public static partial void LogGetHRDashboardFailed(this ILogger logger, string errorMessage);

    [LoggerMessage(
        EventId = 8360,
        Level = LogLevel.Error,
        Message = "Error retrieving HR dashboard")]
    public static partial void LogRetrieveHRDashboardError(this ILogger logger, Exception exception);

    // Auth logging methods (EventIds 8361-8363)
    [LoggerMessage(
        EventId = 8361,
        Level = LogLevel.Warning,
        Message = "User ID not found in claims")]
    public static partial void LogUserIdNotFoundInClaims(this ILogger logger);

    [LoggerMessage(
        EventId = 8362,
        Level = LogLevel.Warning,
        Message = "Employee not found for user ID: {UserId}")]
    public static partial void LogEmployeeNotFoundForUserId(this ILogger logger, Guid userId);

    [LoggerMessage(
        EventId = 8363,
        Level = LogLevel.Error,
        Message = "Error getting application role for user {UserId}")]
    public static partial void LogGetApplicationRoleError(this ILogger logger, Exception exception, Guid userId);

    // Analytics logging methods (EventIds 8364-8365)
    [LoggerMessage(
        EventId = 8364,
        Level = LogLevel.Error,
        Message = "Error retrieving overall analytics")]
    public static partial void LogRetrieveOverallAnalyticsError(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 8365,
        Level = LogLevel.Error,
        Message = "Error retrieving analytics for template {TemplateId}")]
    public static partial void LogRetrieveTemplateAnalyticsError(this ILogger logger, Exception exception, Guid templateId);

    // Command Replay logging methods (EventIds 8366-8367)
    [LoggerMessage(
        EventId = 8366,
        Level = LogLevel.Warning,
        Message = "StartReplay failed: Unable to parse user ID from context")]
    public static partial void LogStartReplayInvalidUserId(this ILogger logger);

    [LoggerMessage(
        EventId = 8367,
        Level = LogLevel.Warning,
        Message = "CancelReplay failed: Unable to parse user ID from context")]
    public static partial void LogCancelReplayInvalidUserId(this ILogger logger);

    // Authorization cache logging method (EventId 8368)
    [LoggerMessage(
        EventId = 8368,
        Level = LogLevel.Error,
        Message = "Error invalidating authorization cache for employee {EmployeeId}")]
    public static partial void LogInvalidateAuthorizationCacheError(this ILogger logger, Exception exception, Guid employeeId);
}