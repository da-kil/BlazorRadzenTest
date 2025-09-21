using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface IQuestionnaireService
{
    // Assignment Operations
    Task<List<QuestionnaireAssignment>> GetAssignmentsAsync(AssignmentFilter? filter = null);
    Task<QuestionnaireAssignment?> GetAssignmentByIdAsync(Guid assignmentId);
    Task<bool> CanAccessAssignmentAsync(Guid assignmentId);

    // Response Operations
    Task<QuestionnaireResponse?> GetResponseAsync(Guid assignmentId);
    Task<QuestionnaireResponse?> SaveResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses);
    Task<QuestionnaireResponse?> SubmitResponseAsync(Guid assignmentId);

    // Template Operations
    Task<List<QuestionnaireTemplate>> GetAccessibleTemplatesAsync();
    Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid templateId);
    Task<QuestionnaireTemplate?> CreateTemplateAsync(QuestionnaireTemplate template);
    Task<bool> UpdateTemplateAsync(QuestionnaireTemplate template);
    Task<bool> DeleteTemplateAsync(Guid templateId);

    // Employee Operations
    Task<List<EmployeeDto>> GetAccessibleEmployeesAsync();
    Task<List<EmployeeDto>> GetTeamMembersAsync();

    // Analytics Operations
    Task<T?> GetAnalyticsAsync<T>(AnalyticsQuery? query = null) where T : class, IAnalytics, new();
    Task<List<TrendData>> GetCompletionTrendsAsync(int days = 30);

    // Progress Operations
    Task<AssignmentProgress?> GetAssignmentProgressAsync(Guid assignmentId);
    Task<List<AssignmentProgress>> GetAllProgressAsync(AssignmentFilter? filter = null);

    // Notification Operations
    Task<bool> SendReminderAsync(Guid assignmentId, string message);
    Task<bool> SendBulkReminderAsync(List<Guid> assignmentIds, string message);

    // Report Operations
    Task<T?> GenerateReportAsync<T>(AnalyticsQuery? query = null) where T : BaseReport, new();
}

public class QuestionnaireService : IQuestionnaireService
{
    private readonly IApiClientService _apiClient;
    private readonly IUserContextService _userContext;
    private readonly ILogger<QuestionnaireService> _logger;

    public QuestionnaireService(
        IApiClientService apiClient,
        IUserContextService userContext,
        ILogger<QuestionnaireService> logger)
    {
        _apiClient = apiClient;
        _userContext = userContext;
        _logger = logger;
    }

    #region Assignment Operations

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsAsync(AssignmentFilter? filter = null)
    {
        try
        {
            var endpoint = ApiEndpoints.Assignments;
            var queryParams = BuildAssignmentFilterParams(filter);

            var assignments = await _apiClient.GetAsync<List<QuestionnaireAssignment>>(endpoint, queryParams);
            return assignments ?? new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching assignments");
            return new List<QuestionnaireAssignment>();
        }
    }

    public async Task<QuestionnaireAssignment?> GetAssignmentByIdAsync(Guid assignmentId)
    {
        if (!await CanAccessAssignmentAsync(assignmentId))
        {
            _logger.LogWarning("Access denied for assignment {AssignmentId}", assignmentId);
            return null;
        }

        var endpoint = _apiClient.BuildEndpoint(ApiEndpoints.AssignmentById, assignmentId);
        return await _apiClient.GetAsync<QuestionnaireAssignment>(endpoint);
    }

    public async Task<bool> CanAccessAssignmentAsync(Guid assignmentId)
    {
        // Admin and HR can access all assignments
        if (_userContext.HasPermission("questionnaire.view.all"))
        {
            return true;
        }

        // For others, check if assignment belongs to accessible scope
        var assignment = await _apiClient.GetAsync<QuestionnaireAssignment>(
            _apiClient.BuildEndpoint(ApiEndpoints.AssignmentById, assignmentId));

        if (assignment == null) return false;

        return _userContext.CanAccessEmployee(assignment.EmployeeId);
    }

    #endregion

    #region Response Operations

    public async Task<QuestionnaireResponse?> GetResponseAsync(Guid assignmentId)
    {
        if (!await CanAccessAssignmentAsync(assignmentId))
        {
            return null;
        }

        var endpoint = _apiClient.BuildEndpoint(ApiEndpoints.Response, assignmentId);
        return await _apiClient.GetAsync<QuestionnaireResponse>(endpoint);
    }

    public async Task<QuestionnaireResponse?> SaveResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses)
    {
        if (!await CanAccessAssignmentAsync(assignmentId))
        {
            return null;
        }

        var endpoint = _apiClient.BuildEndpoint(ApiEndpoints.SaveResponse, assignmentId);
        return await _apiClient.PostAsync<Dictionary<Guid, SectionResponse>, QuestionnaireResponse>(endpoint, sectionResponses);
    }

    public async Task<QuestionnaireResponse?> SubmitResponseAsync(Guid assignmentId)
    {
        if (!await CanAccessAssignmentAsync(assignmentId))
        {
            return null;
        }

        var endpoint = _apiClient.BuildEndpoint(ApiEndpoints.SubmitResponse, assignmentId);
        return await _apiClient.PostAsync<object, QuestionnaireResponse>(endpoint, new { });
    }

    #endregion

    #region Template Operations

    public async Task<List<QuestionnaireTemplate>> GetAccessibleTemplatesAsync()
    {
        var templates = await _apiClient.GetAsync<List<QuestionnaireTemplate>>(ApiEndpoints.Templates);
        return templates ?? new List<QuestionnaireTemplate>();
    }

    public async Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid templateId)
    {
        var endpoint = _apiClient.BuildEndpoint(ApiEndpoints.TemplateById, templateId);
        return await _apiClient.GetAsync<QuestionnaireTemplate>(endpoint);
    }

    public async Task<QuestionnaireTemplate?> CreateTemplateAsync(QuestionnaireTemplate template)
    {
        if (!_userContext.HasPermission("questionnaire.create"))
        {
            _logger.LogWarning("User does not have permission to create templates");
            return null;
        }

        return await _apiClient.PostAsync<QuestionnaireTemplate, QuestionnaireTemplate>(ApiEndpoints.CreateTemplate, template);
    }

    public async Task<bool> UpdateTemplateAsync(QuestionnaireTemplate template)
    {
        if (!_userContext.HasPermission("questionnaire.create"))
        {
            return false;
        }

        var endpoint = _apiClient.BuildEndpoint(ApiEndpoints.UpdateTemplate, template.Id);
        return await _apiClient.PutAsync(endpoint, template);
    }

    public async Task<bool> DeleteTemplateAsync(Guid templateId)
    {
        if (!_userContext.HasPermission("questionnaire.delete"))
        {
            return false;
        }

        var endpoint = _apiClient.BuildEndpoint(ApiEndpoints.TemplateById, templateId);
        return await _apiClient.DeleteAsync(endpoint);
    }

    #endregion

    #region Employee Operations

    public async Task<List<EmployeeDto>> GetAccessibleEmployeesAsync()
    {
        var endpoint = _userContext.CurrentRole switch
        {
            UserRole.Employee => ApiEndpoints.Employees + "?scope=own",
            UserRole.Manager => _apiClient.BuildEndpoint(ApiEndpoints.ManagerTeam, _userContext.GetCurrentUserAsync().Result.UserId),
            UserRole.HR or UserRole.Admin => ApiEndpoints.Employees,
            _ => ApiEndpoints.Employees
        };

        var employees = await _apiClient.GetAsync<List<EmployeeDto>>(endpoint);
        return employees ?? new List<EmployeeDto>();
    }

    public async Task<List<EmployeeDto>> GetTeamMembersAsync()
    {
        if (_userContext.CurrentRole != UserRole.Manager && !_userContext.HasPermission("questionnaire.view.team"))
        {
            return new List<EmployeeDto>();
        }

        var managerId = _userContext.GetCurrentUserAsync().Result.UserId;
        var endpoint = _apiClient.BuildEndpoint(ApiEndpoints.ManagerTeam, managerId);
        var employees = await _apiClient.GetAsync<List<EmployeeDto>>(endpoint);
        return employees ?? new List<EmployeeDto>();
    }

    #endregion

    #region Analytics Operations

    public async Task<T?> GetAnalyticsAsync<T>(AnalyticsQuery? query = null) where T : class, IAnalytics, new()
    {
        var scope = DetermineAnalyticsScope(query?.Scope);
        var endpoint = scope == "hr" ? ApiEndpoints.HROrganizationAnalytics : ApiEndpoints.AnalyticsOverview;

        var queryParams = query != null ? new
        {
            fromDate = query.FromDate?.ToString("yyyy-MM-dd"),
            toDate = query.ToDate?.ToString("yyyy-MM-dd"),
            department = query.Department,
            scope = scope
        } : null;

        return await _apiClient.GetAsync<T>(endpoint, queryParams);
    }

    public async Task<List<TrendData>> GetCompletionTrendsAsync(int days = 30)
    {
        var queryParams = new { days, scope = DetermineAnalyticsScope() };
        var trends = await _apiClient.GetAsync<List<TrendData>>(ApiEndpoints.HRAnalyticsTrends, queryParams);
        return trends ?? new List<TrendData>();
    }

    #endregion

    #region Progress Operations

    public async Task<AssignmentProgress?> GetAssignmentProgressAsync(Guid assignmentId)
    {
        if (!await CanAccessAssignmentAsync(assignmentId))
        {
            return null;
        }

        var endpoint = _apiClient.BuildEndpoint(ApiEndpoints.Progress, assignmentId);
        return await _apiClient.GetAsync<AssignmentProgress>(endpoint);
    }

    public async Task<List<AssignmentProgress>> GetAllProgressAsync(AssignmentFilter? filter = null)
    {
        var queryParams = BuildAssignmentFilterParams(filter);
        var progress = await _apiClient.GetAsync<List<AssignmentProgress>>(ApiEndpoints.AllProgress, queryParams);
        return progress ?? new List<AssignmentProgress>();
    }

    #endregion

    #region Notification Operations

    public async Task<bool> SendReminderAsync(Guid assignmentId, string message)
    {
        if (!await CanAccessAssignmentAsync(assignmentId))
        {
            return false;
        }

        var request = new NotificationRequest
        {
            AssignmentIds = new List<Guid> { assignmentId },
            Message = message,
            Type = NotificationType.Reminder
        };

        return await _apiClient.PostAsync(ApiEndpoints.SendReminder, request);
    }

    public async Task<bool> SendBulkReminderAsync(List<Guid> assignmentIds, string message)
    {
        if (!_userContext.HasPermission("reminder.send.bulk"))
        {
            return false;
        }

        var request = new NotificationRequest
        {
            AssignmentIds = assignmentIds,
            Message = message,
            Type = NotificationType.Reminder
        };

        return await _apiClient.PostAsync(ApiEndpoints.SendBulkReminder, request);
    }

    #endregion

    #region Report Operations

    public async Task<T?> GenerateReportAsync<T>(AnalyticsQuery? query = null) where T : BaseReport, new()
    {
        var reportType = typeof(T).Name.ToLowerInvariant();
        var endpoint = $"q/api/v1/reports/{reportType}";

        var queryParams = query != null ? new
        {
            fromDate = query.FromDate?.ToString("yyyy-MM-dd"),
            toDate = query.ToDate?.ToString("yyyy-MM-dd"),
            department = query.Department,
            scope = DetermineAnalyticsScope(query.Scope)
        } : null;

        return await _apiClient.GetAsync<T>(endpoint, queryParams);
    }

    #endregion

    #region Helper Methods

    private object? BuildAssignmentFilterParams(AssignmentFilter? filter)
    {
        if (filter == null) return null;

        return new
        {
            status = filter.Status?.ToString().ToLowerInvariant(),
            fromDate = filter.FromDate?.ToString("yyyy-MM-dd"),
            toDate = filter.ToDate?.ToString("yyyy-MM-dd"),
            department = filter.Department,
            employeeId = filter.EmployeeId,
            includeTeamOnly = filter.IncludeTeamOnly,
            includeAccessibleOnly = filter.IncludeAccessibleOnly,
            scope = DetermineAnalyticsScope()
        };
    }

    private string DetermineAnalyticsScope(AnalyticsScope? requestedScope = null)
    {
        // Override based on user permissions
        if (_userContext.HasPermission("analytics.view.organization"))
        {
            return requestedScope?.ToString().ToLowerInvariant() ?? "organization";
        }

        if (_userContext.HasPermission("analytics.view.team"))
        {
            return requestedScope == AnalyticsScope.Organization ? "team" : (requestedScope?.ToString().ToLowerInvariant() ?? "team");
        }

        return "own";
    }

    #endregion
}