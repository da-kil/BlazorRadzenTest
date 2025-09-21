using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class ManagerQuestionnaireService : BaseApiService, IManagerQuestionnaireService
{
    private const string ManagerEndpoint = "q/api/v1/managers";
    private const string AssignmentCommandEndpoint = "c/api/v1/assignments";
    private readonly string currentManagerId;

    public ManagerQuestionnaireService(IHttpClientFactory factory) : base(factory)
    {
        // TODO: Get current manager ID from authentication context
        currentManagerId = "current-manager";
    }

    public async Task<List<EmployeeDto>> GetTeamMembersAsync()
    {
        return await GetManagerResourceAsync<EmployeeDto>(ManagerEndpoint, currentManagerId, "team");
    }

    public async Task<List<QuestionnaireAssignment>> GetTeamAssignmentsAsync()
    {
        return await GetManagerResourceAsync<QuestionnaireAssignment>(ManagerEndpoint, currentManagerId, "assignments");
    }

    public async Task<List<QuestionnaireAssignment>> GetTeamAssignmentsByStatusAsync(AssignmentStatus status)
    {
        return await GetManagerResourceWithQueryAsync<QuestionnaireAssignment>(ManagerEndpoint, currentManagerId, "assignments", $"status={status}");
    }

    public async Task<List<AssignmentProgress>> GetTeamProgressAsync()
    {
        return await GetManagerResourceAsync<AssignmentProgress>(ManagerEndpoint, currentManagerId, "team/progress");
    }

    public async Task<TeamAnalytics> GetTeamAnalyticsAsync()
    {
        return await GetManagerSingleResourceAsync<TeamAnalytics>(ManagerEndpoint, currentManagerId, "analytics") ?? new TeamAnalytics();
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByEmployeeAsync(string employeeId)
    {
        return await GetManagerResourceAsync<QuestionnaireAssignment>(ManagerEndpoint, currentManagerId, $"employees/{employeeId}/assignments");
    }

    public async Task<bool> SendReminderAsync(Guid assignmentId, string message)
    {
        var reminderRequest = new
        {
            AssignmentId = assignmentId,
            Message = message,
            SentBy = currentManagerId
        };

        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/reminder", reminderRequest);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error sending reminder for assignment {assignmentId}", ex);
            return false;
        }
    }

    public async Task<TeamPerformanceReport> GenerateTeamReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var queryParams = new List<string>();
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

        var queryString = queryParams.Any() ? string.Join("&", queryParams) : "";

        return await GetManagerSingleResourceAsync<TeamPerformanceReport>(ManagerEndpoint, currentManagerId, $"reports/performance{(string.IsNullOrEmpty(queryString) ? "" : "?" + queryString)}") ?? new TeamPerformanceReport();
    }
}