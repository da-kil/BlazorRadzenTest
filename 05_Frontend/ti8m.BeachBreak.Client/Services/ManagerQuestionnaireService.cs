using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class ManagerQuestionnaireService : IManagerQuestionnaireService
{
    private readonly HttpClient httpQueryClient;
    private readonly HttpClient httpCommandClient;
    private readonly string currentManagerId;

    public ManagerQuestionnaireService(IHttpClientFactory factory)
    {
        httpQueryClient = factory.CreateClient("QueryClient");
        httpCommandClient = factory.CreateClient("CommandClient");
        // TODO: Get current manager ID from authentication context
        currentManagerId = "current-manager";
    }

    public async Task<List<EmployeeDto>> GetTeamMembersAsync()
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<EmployeeDto>>($"q/api/v1/managers/{currentManagerId}/team");
            return response ?? new List<EmployeeDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching team members: {ex.Message}");
            return new List<EmployeeDto>();
        }
    }

    public async Task<List<QuestionnaireAssignment>> GetTeamAssignmentsAsync()
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<QuestionnaireAssignment>>($"q/api/v1/managers/{currentManagerId}/assignments");
            return response ?? new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching team assignments: {ex.Message}");
            return new List<QuestionnaireAssignment>();
        }
    }

    public async Task<List<QuestionnaireAssignment>> GetTeamAssignmentsByStatusAsync(AssignmentStatus status)
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<QuestionnaireAssignment>>($"q/api/v1/managers/{currentManagerId}/assignments?status={status}");
            return response ?? new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching team assignments by status {status}: {ex.Message}");
            return new List<QuestionnaireAssignment>();
        }
    }

    public async Task<List<AssignmentProgress>> GetTeamProgressAsync()
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<AssignmentProgress>>($"q/api/v1/managers/{currentManagerId}/team/progress");
            return response ?? new List<AssignmentProgress>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching team progress: {ex.Message}");
            return new List<AssignmentProgress>();
        }
    }

    public async Task<TeamAnalytics> GetTeamAnalyticsAsync()
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<TeamAnalytics>($"q/api/v1/managers/{currentManagerId}/analytics");
            return response ?? new TeamAnalytics();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching team analytics: {ex.Message}");
            return new TeamAnalytics();
        }
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByEmployeeAsync(string employeeId)
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<QuestionnaireAssignment>>($"q/api/v1/managers/{currentManagerId}/employees/{employeeId}/assignments");
            return response ?? new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching assignments for employee {employeeId}: {ex.Message}");
            return new List<QuestionnaireAssignment>();
        }
    }

    public async Task<bool> SendReminderAsync(Guid assignmentId, string message)
    {
        try
        {
            var reminderRequest = new
            {
                AssignmentId = assignmentId,
                Message = message,
                SentBy = currentManagerId
            };

            var response = await httpCommandClient.PostAsJsonAsync("c/api/v1/assignments/reminder", reminderRequest);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending reminder for assignment {assignmentId}: {ex.Message}");
            return false;
        }
    }

    public async Task<TeamPerformanceReport> GenerateTeamReportAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";

            var response = await httpQueryClient.GetFromJsonAsync<TeamPerformanceReport>($"q/api/v1/managers/{currentManagerId}/reports/performance{queryString}");
            return response ?? new TeamPerformanceReport();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating team performance report: {ex.Message}");
            return new TeamPerformanceReport();
        }
    }
}