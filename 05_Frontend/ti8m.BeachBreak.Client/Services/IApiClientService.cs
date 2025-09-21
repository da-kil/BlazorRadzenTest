using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ti8m.BeachBreak.Client.Services;

public interface IApiClientService
{
    Task<T?> GetAsync<T>(string endpoint, object? parameters = null);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request);
    Task<bool> PostAsync<TRequest>(string endpoint, TRequest request);
    Task<bool> PutAsync<TRequest>(string endpoint, TRequest request);
    Task<bool> DeleteAsync(string endpoint);
    string BuildEndpoint(string template, params object[] parameters);
}

public class ApiClientService : IApiClientService
{
    private readonly HttpClient _queryClient;
    private readonly HttpClient _commandClient;
    private readonly IUserContextService _userContext;
    private readonly ILogger<ApiClientService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClientService(
        IHttpClientFactory httpClientFactory,
        IUserContextService userContext,
        ILogger<ApiClientService> logger)
    {
        _queryClient = httpClientFactory.CreateClient("QueryClient");
        _commandClient = httpClientFactory.CreateClient("CommandClient");
        _userContext = userContext;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<T?> GetAsync<T>(string endpoint, object? parameters = null)
    {
        try
        {
            var fullEndpoint = BuildEndpointWithQuery(endpoint, parameters);
            _logger.LogInformation("GET request to {Endpoint}", fullEndpoint);

            var response = await _queryClient.GetAsync(fullEndpoint);
            await HandleResponseErrors(response, fullEndpoint);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default(T);
            }

            var result = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
            _logger.LogInformation("GET request successful for {Endpoint}", fullEndpoint);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GET request to {Endpoint}", endpoint);
            return default(T);
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        try
        {
            _logger.LogInformation("POST request to {Endpoint}", endpoint);

            var response = await _commandClient.PostAsJsonAsync(endpoint, request, _jsonOptions);
            await HandleResponseErrors(response, endpoint);

            var result = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
            _logger.LogInformation("POST request successful for {Endpoint}", endpoint);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in POST request to {Endpoint}", endpoint);
            return default(TResponse);
        }
    }

    public async Task<bool> PostAsync<TRequest>(string endpoint, TRequest request)
    {
        try
        {
            _logger.LogInformation("POST request to {Endpoint}", endpoint);

            var response = await _commandClient.PostAsJsonAsync(endpoint, request, _jsonOptions);
            var isSuccess = response.IsSuccessStatusCode;

            if (isSuccess)
            {
                _logger.LogInformation("POST request successful for {Endpoint}", endpoint);
            }
            else
            {
                await HandleResponseErrors(response, endpoint);
            }

            return isSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in POST request to {Endpoint}", endpoint);
            return false;
        }
    }

    public async Task<bool> PutAsync<TRequest>(string endpoint, TRequest request)
    {
        try
        {
            _logger.LogInformation("PUT request to {Endpoint}", endpoint);

            var response = await _commandClient.PutAsJsonAsync(endpoint, request, _jsonOptions);
            var isSuccess = response.IsSuccessStatusCode;

            if (isSuccess)
            {
                _logger.LogInformation("PUT request successful for {Endpoint}", endpoint);
            }
            else
            {
                await HandleResponseErrors(response, endpoint);
            }

            return isSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in PUT request to {Endpoint}", endpoint);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            _logger.LogInformation("DELETE request to {Endpoint}", endpoint);

            var response = await _commandClient.DeleteAsync(endpoint);
            var isSuccess = response.IsSuccessStatusCode;

            if (isSuccess)
            {
                _logger.LogInformation("DELETE request successful for {Endpoint}", endpoint);
            }
            else
            {
                await HandleResponseErrors(response, endpoint);
            }

            return isSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DELETE request to {Endpoint}", endpoint);
            return false;
        }
    }

    public string BuildEndpoint(string template, params object[] parameters)
    {
        var endpoint = template;
        for (int i = 0; i < parameters.Length; i++)
        {
            endpoint = endpoint.Replace($"{{{i}}}", Uri.EscapeDataString(parameters[i]?.ToString() ?? string.Empty));
        }

        // Add user context if needed
        var currentUser = _userContext.GetCurrentUserAsync().Result;
        endpoint = endpoint.Replace("{userId}", currentUser.UserId)
                          .Replace("{employeeId}", currentUser.EmployeeId)
                          .Replace("{role}", currentUser.Role.ToString().ToLower());

        return endpoint;
    }

    private string BuildEndpointWithQuery(string endpoint, object? parameters)
    {
        var fullEndpoint = endpoint;

        if (parameters != null)
        {
            var queryString = BuildQueryString(parameters);
            if (!string.IsNullOrEmpty(queryString))
            {
                fullEndpoint += (endpoint.Contains('?') ? "&" : "?") + queryString;
            }
        }

        return fullEndpoint;
    }

    private static string BuildQueryString(object parameters)
    {
        var properties = parameters.GetType().GetProperties()
            .Where(p => p.GetValue(parameters) != null)
            .Select(p => $"{p.Name.ToLowerInvariant()}={Uri.EscapeDataString(p.GetValue(parameters)?.ToString() ?? string.Empty)}");

        return string.Join("&", properties);
    }

    private async Task HandleResponseErrors(HttpResponseMessage response, string endpoint)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorMessage = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {errorContent}";

            _logger.LogError("API request failed for {Endpoint}: {Error}", endpoint, errorMessage);

            // Don't throw exceptions, let the calling code handle the error
            // This prevents cascading failures and provides better UX
        }
    }
}

public class ApiEndpoints
{
    // Assignment endpoints
    public const string Assignments = "q/api/v1/assignments";
    public const string AssignmentById = "q/api/v1/assignments/{0}";
    public const string AssignmentsByEmployee = "q/api/v1/assignments/employee/{0}";
    public const string AssignmentsByStatus = "q/api/v1/assignments/status/{0}";
    public const string AssignmentsByDateRange = "q/api/v1/assignments/daterange";

    // Response endpoints
    public const string Response = "q/api/v1/responses/assignment/{0}";
    public const string SaveResponse = "c/api/v1/responses/assignment/{0}";
    public const string SubmitResponse = "c/api/v1/responses/assignment/{0}/submit";

    // Analytics endpoints
    public const string AnalyticsOverview = "q/api/v1/analytics/overview";
    public const string AnalyticsTemplate = "q/api/v1/analytics/template/{0}";

    // Template endpoints
    public const string Templates = "q/api/v1/questionnaire-templates";
    public const string TemplateById = "q/api/v1/questionnaire-templates/{0}";
    public const string CreateTemplate = "c/api/v1/questionnaire-templates";
    public const string UpdateTemplate = "c/api/v1/questionnaire-templates/{0}";

    // Employee endpoints
    public const string Employees = "q/api/v1/employees";
    public const string EmployeeById = "q/api/v1/employees/{0}";
    public const string EmployeeAssignments = "q/api/v1/employees/{0}/assignments";
    public const string EmployeeAssignmentById = "q/api/v1/employees/{0}/assignments/{1}";
    public const string EmployeeResponse = "q/api/v1/employees/{0}/responses/assignment/{1}";
    public const string EmployeeProgress = "q/api/v1/employees/{0}/assignments/progress";
    public const string EmployeeAssignmentProgress = "q/api/v1/employees/{0}/assignments/{1}/progress";

    // Manager endpoints
    public const string ManagerTeam = "q/api/v1/managers/{0}/team";
    public const string ManagerAssignments = "q/api/v1/managers/{0}/assignments";
    public const string ManagerTeamProgress = "q/api/v1/managers/{0}/team/progress";
    public const string ManagerAnalytics = "q/api/v1/managers/{0}/analytics";
    public const string ManagerEmployeeAssignments = "q/api/v1/managers/{0}/employees/{1}/assignments";
    public const string ManagerPerformanceReport = "q/api/v1/managers/{0}/reports/performance";

    // HR endpoints
    public const string HREmployees = "q/api/v1/hr/employees";
    public const string HRAssignments = "q/api/v1/hr/assignments";
    public const string HRAssignmentsByDepartment = "q/api/v1/hr/assignments/department/{0}";
    public const string HROrganizationAnalytics = "q/api/v1/hr/analytics/organization";
    public const string HRDepartmentAnalytics = "q/api/v1/hr/analytics/departments";
    public const string HRCompliance = "q/api/v1/hr/compliance";
    public const string HROrganizationReport = "q/api/v1/hr/reports/organization";
    public const string HRQuestionnaireUsageStats = "q/api/v1/hr/questionnaires/usage-stats";
    public const string HRAnalyticsTrends = "q/api/v1/hr/analytics/trends";

    // Notification endpoints
    public const string SendReminder = "c/api/v1/notifications/reminder";
    public const string SendBulkReminder = "c/api/v1/notifications/bulk-reminder";

    // Progress endpoints
    public const string Progress = "q/api/v1/progress/assignment/{0}";
    public const string AllProgress = "q/api/v1/progress";
}