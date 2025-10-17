using Microsoft.Extensions.Logging;

namespace ti8m.BeachBreak.Application.Query;

public static partial class LoggerMessageDefinitions
{
    // Organization Query Operations
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Information,
        Message = "Starting organization list query with filters - IncludeDeleted: {IncludeDeleted}, IncludeIgnored: {IncludeIgnored}, ParentId: {ParentId}, ManagerId: {ManagerId}")]
    public static partial void LogOrganizationListQueryStarting(this ILogger logger, bool includeDeleted, bool includeIgnored, Guid? parentId, string? managerId);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Information,
        Message = "Successfully retrieved {Count} organizations")]
    public static partial void LogOrganizationListQuerySucceeded(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Error,
        Message = "Error occurred while retrieving organization list")]
    public static partial void LogOrganizationListQueryFailed(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Information,
        Message = "Starting organization query for Id: {Id}")]
    public static partial void LogOrganizationQueryStarting(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 4005,
        Level = LogLevel.Information,
        Message = "Successfully retrieved organization with Id: {Id}")]
    public static partial void LogOrganizationQuerySucceeded(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 4006,
        Level = LogLevel.Warning,
        Message = "Organization with Id {Id} not found")]
    public static partial void LogOrganizationNotFound(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 4007,
        Level = LogLevel.Error,
        Message = "Error occurred while retrieving organization with Id: {Id}")]
    public static partial void LogOrganizationQueryFailed(this ILogger logger, Guid id, Exception exception);

    [LoggerMessage(
        EventId = 4008,
        Level = LogLevel.Information,
        Message = "Starting organization query for Number: {Number}")]
    public static partial void LogOrganizationByNumberQueryStarting(this ILogger logger, string number);

    [LoggerMessage(
        EventId = 4009,
        Level = LogLevel.Information,
        Message = "Successfully retrieved organization with Number: {Number}")]
    public static partial void LogOrganizationByNumberQuerySucceeded(this ILogger logger, string number);

    [LoggerMessage(
        EventId = 4010,
        Level = LogLevel.Warning,
        Message = "Organization with Number {Number} not found")]
    public static partial void LogOrganizationByNumberNotFound(this ILogger logger, string number);

    [LoggerMessage(
        EventId = 4011,
        Level = LogLevel.Error,
        Message = "Error occurred while retrieving organization with Number: {Number}")]
    public static partial void LogOrganizationByNumberQueryFailed(this ILogger logger, string number, Exception exception);

    // Employee Query Operations
    [LoggerMessage(
        EventId = 4012,
        Level = LogLevel.Information,
        Message = "Starting employee list query with filters - IncludeDeleted: {IncludeDeleted}, OrganizationNumber: {OrganizationNumber}, Role: {Role}, ManagerId: {ManagerId}")]
    public static partial void LogEmployeeListQueryStarting(this ILogger logger, bool includeDeleted, int? organizationNumber, string? role, Guid? managerId);

    [LoggerMessage(
        EventId = 4013,
        Level = LogLevel.Information,
        Message = "Employee list query completed successfully, returned {EmployeeCount} employees")]
    public static partial void LogEmployeeListQuerySucceeded(this ILogger logger, int employeeCount);

    [LoggerMessage(
        EventId = 4014,
        Level = LogLevel.Error,
        Message = "Failed to execute employee list query")]
    public static partial void LogEmployeeListQueryFailed(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 4015,
        Level = LogLevel.Information,
        Message = "Starting single employee query for EmployeeId: {EmployeeId}")]
    public static partial void LogEmployeeQueryStarting(this ILogger logger, Guid employeeId);

    [LoggerMessage(
        EventId = 4016,
        Level = LogLevel.Information,
        Message = "Single employee query completed successfully for EmployeeId: {EmployeeId}")]
    public static partial void LogEmployeeQuerySucceeded(this ILogger logger, Guid employeeId);

    [LoggerMessage(
        EventId = 4017,
        Level = LogLevel.Information,
        Message = "Employee not found for EmployeeId: {EmployeeId}")]
    public static partial void LogEmployeeNotFound(this ILogger logger, Guid employeeId);

    [LoggerMessage(
        EventId = 4018,
        Level = LogLevel.Error,
        Message = "Failed to execute single employee query for EmployeeId: {EmployeeId}")]
    public static partial void LogEmployeeQueryFailed(this ILogger logger, Guid employeeId, Exception exception);

    // Category Query Operations
    [LoggerMessage(
        EventId = 4019,
        Level = LogLevel.Information,
        Message = "Starting category list query")]
    public static partial void LogCategoryListQueryStarting(this ILogger logger);

    [LoggerMessage(
        EventId = 4020,
        Level = LogLevel.Information,
        Message = "Category list query completed successfully, returned {CategoryCount} categories")]
    public static partial void LogCategoryListQuerySucceeded(this ILogger logger, int categoryCount);

    [LoggerMessage(
        EventId = 4021,
        Level = LogLevel.Error,
        Message = "Failed to retrieve categories")]
    public static partial void LogCategoryListQueryFailed(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 4022,
        Level = LogLevel.Information,
        Message = "Starting category query for CategoryId: {CategoryId}")]
    public static partial void LogCategoryQueryStarting(this ILogger logger, Guid categoryId);

    [LoggerMessage(
        EventId = 4023,
        Level = LogLevel.Information,
        Message = "Category query completed successfully for CategoryId: {CategoryId}")]
    public static partial void LogCategoryQuerySucceeded(this ILogger logger, Guid categoryId);

    [LoggerMessage(
        EventId = 4024,
        Level = LogLevel.Error,
        Message = "Failed to retrieve category with ID {Id}")]
    public static partial void LogCategoryQueryFailed(this ILogger logger, Guid id, Exception exception);

    // Questionnaire Template Query Operations
    [LoggerMessage(
        EventId = 4025,
        Level = LogLevel.Information,
        Message = "Starting questionnaire template list query")]
    public static partial void LogQuestionnaireTemplateListQueryStarting(this ILogger logger);

    [LoggerMessage(
        EventId = 4026,
        Level = LogLevel.Information,
        Message = "Retrieved {Count} questionnaire templates")]
    public static partial void LogQuestionnaireTemplateListQuerySucceeded(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 4027,
        Level = LogLevel.Error,
        Message = "Failed to retrieve questionnaire templates")]
    public static partial void LogQuestionnaireTemplateListQueryFailed(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 4028,
        Level = LogLevel.Information,
        Message = "Starting questionnaire template query for Id: {Id}")]
    public static partial void LogQuestionnaireTemplateQueryStarting(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 4029,
        Level = LogLevel.Information,
        Message = "Retrieved questionnaire template with ID {Id}")]
    public static partial void LogQuestionnaireTemplateQuerySucceeded(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 4030,
        Level = LogLevel.Warning,
        Message = "Questionnaire template with ID {Id} not found")]
    public static partial void LogQuestionnaireTemplateNotFound(this ILogger logger, Guid id);

    [LoggerMessage(
        EventId = 4031,
        Level = LogLevel.Error,
        Message = "Failed to retrieve questionnaire template with ID {Id}")]
    public static partial void LogQuestionnaireTemplateQueryFailed(this ILogger logger, Guid id, Exception exception);

    [LoggerMessage(
        EventId = 4032,
        Level = LogLevel.Information,
        Message = "Starting published questionnaire templates query")]
    public static partial void LogPublishedQuestionnaireTemplatesQueryStarting(this ILogger logger);

    [LoggerMessage(
        EventId = 4033,
        Level = LogLevel.Information,
        Message = "Retrieved {Count} published questionnaire templates")]
    public static partial void LogPublishedQuestionnaireTemplatesQuerySucceeded(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 4034,
        Level = LogLevel.Error,
        Message = "Failed to retrieve published questionnaire templates")]
    public static partial void LogPublishedQuestionnaireTemplatesQueryFailed(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 4035,
        Level = LogLevel.Information,
        Message = "Starting draft questionnaire templates query")]
    public static partial void LogDraftQuestionnaireTemplatesQueryStarting(this ILogger logger);

    [LoggerMessage(
        EventId = 4036,
        Level = LogLevel.Information,
        Message = "Retrieved {Count} draft questionnaire templates")]
    public static partial void LogDraftQuestionnaireTemplatesQuerySucceeded(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 4037,
        Level = LogLevel.Error,
        Message = "Failed to retrieve draft questionnaire templates")]
    public static partial void LogDraftQuestionnaireTemplatesQueryFailed(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 4038,
        Level = LogLevel.Information,
        Message = "Starting archived questionnaire templates query")]
    public static partial void LogArchivedQuestionnaireTemplatesQueryStarting(this ILogger logger);

    [LoggerMessage(
        EventId = 4039,
        Level = LogLevel.Information,
        Message = "Retrieved {Count} archived questionnaire templates")]
    public static partial void LogArchivedQuestionnaireTemplatesQuerySucceeded(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 4040,
        Level = LogLevel.Error,
        Message = "Failed to retrieve archived questionnaire templates")]
    public static partial void LogArchivedQuestionnaireTemplatesQueryFailed(this ILogger logger, Exception exception);

    [LoggerMessage(
        EventId = 4041,
        Level = LogLevel.Information,
        Message = "Starting assignable questionnaire templates query")]
    public static partial void LogAssignableQuestionnaireTemplatesQueryStarting(this ILogger logger);

    [LoggerMessage(
        EventId = 4042,
        Level = LogLevel.Information,
        Message = "Retrieved {Count} assignable questionnaire templates")]
    public static partial void LogAssignableQuestionnaireTemplatesQuerySucceeded(this ILogger logger, int count);

    [LoggerMessage(
        EventId = 4043,
        Level = LogLevel.Error,
        Message = "Failed to retrieve assignable questionnaire templates")]
    public static partial void LogAssignableQuestionnaireTemplatesQueryFailed(this ILogger logger, Exception exception);

    // Employee Dashboard Query Operations
    [LoggerMessage(
        EventId = 4044,
        Level = LogLevel.Information,
        Message = "Starting employee dashboard query for EmployeeId: {EmployeeId}")]
    public static partial void LogEmployeeDashboardQueryStarting(this ILogger logger, Guid employeeId);

    [LoggerMessage(
        EventId = 4045,
        Level = LogLevel.Information,
        Message = "Employee dashboard query completed successfully for EmployeeId: {EmployeeId}")]
    public static partial void LogEmployeeDashboardQuerySucceeded(this ILogger logger, Guid employeeId);

    [LoggerMessage(
        EventId = 4046,
        Level = LogLevel.Warning,
        Message = "Employee dashboard not found for EmployeeId: {EmployeeId}")]
    public static partial void LogEmployeeDashboardNotFound(this ILogger logger, Guid employeeId);

    [LoggerMessage(
        EventId = 4047,
        Level = LogLevel.Error,
        Message = "Failed to retrieve employee dashboard for EmployeeId: {EmployeeId}")]
    public static partial void LogEmployeeDashboardQueryFailed(this ILogger logger, Guid employeeId, Exception exception);

    // Manager Dashboard Query Operations
    [LoggerMessage(
        EventId = 4048,
        Level = LogLevel.Information,
        Message = "Starting manager dashboard query for ManagerId: {ManagerId}")]
    public static partial void LogManagerDashboardQueryStarting(this ILogger logger, Guid managerId);

    [LoggerMessage(
        EventId = 4049,
        Level = LogLevel.Information,
        Message = "Manager dashboard query completed successfully for ManagerId: {ManagerId}")]
    public static partial void LogManagerDashboardQuerySucceeded(this ILogger logger, Guid managerId);

    [LoggerMessage(
        EventId = 4050,
        Level = LogLevel.Warning,
        Message = "Manager dashboard not found for ManagerId: {ManagerId}")]
    public static partial void LogManagerDashboardNotFound(this ILogger logger, Guid managerId);

    [LoggerMessage(
        EventId = 4051,
        Level = LogLevel.Error,
        Message = "Failed to retrieve manager dashboard for ManagerId: {ManagerId}")]
    public static partial void LogManagerDashboardQueryFailed(this ILogger logger, Guid managerId, Exception exception);

    // HR Dashboard Query Operations
    [LoggerMessage(
        EventId = 4052,
        Level = LogLevel.Information,
        Message = "Starting HR dashboard query")]
    public static partial void LogHRDashboardQueryStarting(this ILogger logger);

    [LoggerMessage(
        EventId = 4053,
        Level = LogLevel.Information,
        Message = "HR dashboard query completed successfully")]
    public static partial void LogHRDashboardQuerySucceeded(this ILogger logger);

    [LoggerMessage(
        EventId = 4054,
        Level = LogLevel.Warning,
        Message = "HR dashboard not found")]
    public static partial void LogHRDashboardNotFound(this ILogger logger);

    [LoggerMessage(
        EventId = 4055,
        Level = LogLevel.Error,
        Message = "Failed to retrieve HR dashboard")]
    public static partial void LogHRDashboardQueryFailed(this ILogger logger, Exception exception);
}