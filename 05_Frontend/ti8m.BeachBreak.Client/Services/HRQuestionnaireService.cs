using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class HRQuestionnaireService : IHRQuestionnaireService
{
    private readonly HttpClient httpQueryClient;
    private readonly HttpClient httpCommandClient;
    private readonly string currentHRUserId;

    public HRQuestionnaireService(IHttpClientFactory factory)
    {
        httpQueryClient = factory.CreateClient("QueryClient");
        httpCommandClient = factory.CreateClient("CommandClient");
        // TODO: Get current HR user ID from authentication context
        currentHRUserId = "current-hr-user";
    }

    public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<EmployeeDto>>("q/api/v1/hr/employees");
            return response ?? new List<EmployeeDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching all employees: {ex.Message}");
            return new List<EmployeeDto>();
        }
    }

    public async Task<List<QuestionnaireAssignment>> GetAllAssignmentsAsync()
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<QuestionnaireAssignment>>("q/api/v1/hr/assignments");
            return response ?? new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching all assignments: {ex.Message}");
            return new List<QuestionnaireAssignment>();
        }
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByDepartmentAsync(string department)
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<QuestionnaireAssignment>>($"q/api/v1/hr/assignments/department/{Uri.EscapeDataString(department)}");
            return response ?? new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching assignments for department {department}: {ex.Message}");
            return new List<QuestionnaireAssignment>();
        }
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<QuestionnaireAssignment>>($"q/api/v1/hr/assignments?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");
            return response ?? new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching assignments by date range: {ex.Message}");
            return new List<QuestionnaireAssignment>();
        }
    }

    public async Task<OrganizationAnalytics> GetOrganizationAnalyticsAsync()
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<OrganizationAnalytics>("q/api/v1/hr/analytics/organization");
            return response ?? new OrganizationAnalytics();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching organization analytics: {ex.Message}");
            return new OrganizationAnalytics();
        }
    }

    public async Task<List<DepartmentAnalytics>> GetDepartmentAnalyticsAsync()
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<DepartmentAnalytics>>("q/api/v1/hr/analytics/departments");
            return response ?? new List<DepartmentAnalytics>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching department analytics: {ex.Message}");
            return new List<DepartmentAnalytics>();
        }
    }

    public async Task<ComplianceReport> GetComplianceReportAsync()
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<ComplianceReport>("q/api/v1/hr/compliance");
            return response ?? new ComplianceReport();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching compliance report: {ex.Message}");
            return new ComplianceReport();
        }
    }

    public async Task<OrganizationReport> GenerateOrganizationReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";

            var response = await httpQueryClient.GetFromJsonAsync<OrganizationReport>($"q/api/v1/hr/reports/organization{queryString}");
            return response ?? new OrganizationReport();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating organization report: {ex.Message}");
            return new OrganizationReport();
        }
    }

    public async Task<bool> SendBulkReminderAsync(List<Guid> assignmentIds, string message)
    {
        try
        {
            var bulkReminderRequest = new
            {
                AssignmentIds = assignmentIds,
                Message = message,
                SentBy = currentHRUserId
            };

            var response = await httpCommandClient.PostAsJsonAsync("c/api/v1/assignments/bulk-reminder", bulkReminderRequest);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending bulk reminder: {ex.Message}");
            return false;
        }
    }

    public async Task<List<QuestionnaireTemplate>> GetQuestionnaireUsageStatsAsync()
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<QuestionnaireTemplate>>("q/api/v1/hr/questionnaires/usage-stats");
            return response ?? new List<QuestionnaireTemplate>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching questionnaire usage stats: {ex.Message}");
            return new List<QuestionnaireTemplate>();
        }
    }

    public async Task<List<TrendData>> GetCompletionTrendsAsync(int days = 30)
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<TrendData>>($"q/api/v1/hr/analytics/trends?days={days}");
            return response ?? new List<TrendData>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching completion trends: {ex.Message}");
            return new List<TrendData>();
        }
    }
}