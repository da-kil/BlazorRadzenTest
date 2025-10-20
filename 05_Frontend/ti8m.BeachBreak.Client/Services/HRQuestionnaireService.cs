using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class HRQuestionnaireService : BaseApiService, IHRQuestionnaireService
{
    private const string HREndpoint = "q/api/v1/hr";
    private const string EmployeesEndpoint = "q/api/v1/employees";
    private const string AssignmentsEndpoint = "q/api/v1/assignments";
    private readonly string currentHRUserId;

    public HRQuestionnaireService(IHttpClientFactory factory) : base(factory)
    {
        // TODO: Get current HR user ID from authentication context
        currentHRUserId = "current-hr-user";
    }

    public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
    {
        // Use the employees endpoint directly, not HR sub-path
        return await GetAllAsync<EmployeeDto>(EmployeesEndpoint);
    }

    public async Task<List<QuestionnaireAssignment>> GetAllAssignmentsAsync()
    {
        // Use the assignments endpoint directly, not HR sub-path
        return await GetAllAsync<QuestionnaireAssignment>(AssignmentsEndpoint);
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByDepartmentAsync(string department)
    {
        return await GetHRResourceWithSubPathAsync<QuestionnaireAssignment>(HREndpoint, "assignments", "department", department);
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        var queryString = $"fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
        return await GetAllAsync<QuestionnaireAssignment>($"{HREndpoint}/assignments", queryString);
    }

    public async Task<OrganizationAnalytics> GetOrganizationAnalyticsAsync()
    {
        return await GetHRSingleResourceAsync<OrganizationAnalytics>(HREndpoint, "analytics/organization") ?? new OrganizationAnalytics();
    }

    public async Task<List<DepartmentAnalytics>> GetDepartmentAnalyticsAsync()
    {
        return await GetHRResourceAsync<DepartmentAnalytics>(HREndpoint, "analytics/departments");
    }

    public async Task<ComplianceReport> GetComplianceReportAsync()
    {
        return await GetHRSingleResourceAsync<ComplianceReport>(HREndpoint, "compliance") ?? new ComplianceReport();
    }

    public async Task<OrganizationReport> GenerateOrganizationReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var queryParams = new List<string>();
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

        var queryString = queryParams.Any() ? string.Join("&", queryParams) : "";

        return await GetHRSingleResourceAsync<OrganizationReport>(HREndpoint, $"reports/organization{(string.IsNullOrEmpty(queryString) ? "" : "?" + queryString)}") ?? new OrganizationReport();
    }

    public async Task<bool> SendBulkReminderAsync(List<Guid> assignmentIds, string message)
    {
        var bulkReminderRequest = new
        {
            AssignmentIds = assignmentIds,
            Message = message,
            SentBy = currentHRUserId
        };

        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync("c/api/v1/assignments/bulk-reminder", bulkReminderRequest);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError("Error sending bulk reminder", ex);
            return false;
        }
    }

    public async Task<List<QuestionnaireTemplate>> GetQuestionnaireUsageStatsAsync()
    {
        return await GetHRResourceAsync<QuestionnaireTemplate>(HREndpoint, "questionnaires/usage-stats");
    }

    public async Task<List<TrendData>> GetCompletionTrendsAsync(int days = 30)
    {
        var queryString = $"days={days}";
        return await GetAllAsync<TrendData>($"{HREndpoint}/analytics/trends", queryString);
    }
}