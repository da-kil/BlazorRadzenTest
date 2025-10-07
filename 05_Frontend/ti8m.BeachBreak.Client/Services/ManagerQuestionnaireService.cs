using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class ManagerQuestionnaireService : BaseApiService, IManagerQuestionnaireService
{
    private const string ManagerEndpoint = "q/api/v1/managers";
    private const string AssignmentCommandEndpoint = "c/api/v1/assignments";

    public ManagerQuestionnaireService(IHttpClientFactory factory) : base(factory)
    {
    }

    public async Task<List<EmployeeDto>> GetTeamMembersAsync()
    {
        // Uses authenticated manager ID from UserContext on backend
        return await GetAllAsync<EmployeeDto>($"{ManagerEndpoint}/me/team");
    }

    public async Task<List<QuestionnaireAssignment>> GetTeamAssignmentsAsync()
    {
        // Uses authenticated manager ID from UserContext on backend
        return await GetAllAsync<QuestionnaireAssignment>($"{ManagerEndpoint}/me/assignments");
    }

    public async Task<List<QuestionnaireAssignment>> GetTeamAssignmentsByStatusAsync(AssignmentStatus status)
    {
        // Uses authenticated manager ID from UserContext on backend
        return await GetAllAsync<QuestionnaireAssignment>($"{ManagerEndpoint}/me/assignments", $"status={status}");
    }

    public async Task<List<AssignmentProgress>> GetTeamProgressAsync()
    {
        // Uses authenticated manager ID from UserContext on backend
        return await GetAllAsync<AssignmentProgress>($"{ManagerEndpoint}/me/team/progress");
    }

    public async Task<TeamAnalytics> GetTeamAnalyticsAsync()
    {
        // Uses authenticated manager ID from UserContext on backend
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<TeamAnalytics>($"{ManagerEndpoint}/me/analytics") ?? new TeamAnalytics();
        }
        catch (Exception ex)
        {
            LogError("Error fetching team analytics", ex);
            return new TeamAnalytics();
        }
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByEmployeeAsync(string employeeId)
    {
        // Uses EmployeesController endpoint which has proper authorization
        return await GetEmployeeResourceAsync<QuestionnaireAssignment>("q/api/v1/employees", employeeId, "assignments");
    }

    public async Task<bool> SendReminderAsync(Guid assignmentId, string message)
    {
        var reminderRequest = new
        {
            AssignmentId = assignmentId,
            Message = message,
            SentBy = "System" // Backend will use authenticated user context
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
        // TODO: Implement when backend endpoint is available
        return new TeamPerformanceReport();
    }
}