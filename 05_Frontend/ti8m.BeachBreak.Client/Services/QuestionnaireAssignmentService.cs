using ti8m.BeachBreak.Client.Models;
using System.Net.Http.Json;

namespace ti8m.BeachBreak.Client.Services;

public class QuestionnaireAssignmentService : BaseApiService, IQuestionnaireAssignmentService
{
    private const string AssignmentQueryEndpoint = "q/api/v1/assignments";
    private const string AssignmentCommandEndpoint = "c/api/v1/assignments";
    private const string AnalyticsEndpoint = "q/api/v1/analytics";

    public QuestionnaireAssignmentService(IHttpClientFactory factory) : base(factory)
    {
    }

    // Assignment CRUD operations
    public async Task<List<QuestionnaireAssignment>> GetAllAssignmentsAsync()
    {
        return await GetAllAsync<QuestionnaireAssignment>(AssignmentQueryEndpoint);
    }

    public async Task<QuestionnaireAssignment?> GetAssignmentByIdAsync(Guid id)
    {
        return await GetByIdAsync<QuestionnaireAssignment>(AssignmentQueryEndpoint, id);
    }

    public async Task<bool> DeleteAssignmentAsync(Guid id)
    {
        return await DeleteAsync(AssignmentCommandEndpoint, id);
    }

    // Assignment creation and management
    public async Task<List<QuestionnaireAssignment>> CreateAssignmentsAsync(
        Guid templateId,
        List<string> employeeIds,
        DateTime? dueDate,
        string? notes,
        string assignedBy)
    {
        var createRequest = new
        {
            TemplateId = templateId,
            EmployeeIds = employeeIds,
            DueDate = dueDate,
            Notes = notes,
            AssignedBy = assignedBy
        };

        return await CreateWithListResponseAsync<object, QuestionnaireAssignment>(AssignmentCommandEndpoint, createRequest);
    }

    public async Task<QuestionnaireAssignment?> UpdateAssignmentStatusAsync(Guid id, AssignmentStatus status)
    {
        return await PatchAsync<AssignmentStatus, QuestionnaireAssignment>(AssignmentCommandEndpoint, id, "status", status);
    }

    // Assignment queries
    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByEmployeeAsync(string employeeId)
    {
        return await GetAllAsync<QuestionnaireAssignment>($"{AssignmentQueryEndpoint}/employee/{employeeId}");
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByTemplateAsync(Guid templateId)
    {
        return await GetAllAsync<QuestionnaireAssignment>($"{AssignmentQueryEndpoint}/template/{templateId}");
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByStatusAsync(AssignmentStatus status)
    {
        return await GetAllAsync<QuestionnaireAssignment>($"{AssignmentQueryEndpoint}/status/{status}");
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByAssignerAsync(string assignerId)
    {
        return await GetAllAsync<QuestionnaireAssignment>($"{AssignmentQueryEndpoint}/assigner/{assignerId}");
    }

    // Assignment analytics
    public async Task<Dictionary<string, object>> GetAssignmentAnalyticsAsync()
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<Dictionary<string, object>>($"{AnalyticsEndpoint}/assignments") ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            LogError("Error fetching assignment analytics", ex);
            return new Dictionary<string, object>();
        }
    }

    public async Task<Dictionary<string, object>> GetEmployeeAssignmentStatsAsync(string employeeId)
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<Dictionary<string, object>>($"{AnalyticsEndpoint}/assignments/employee/{employeeId}") ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            LogError($"Error fetching assignment stats for employee {employeeId}", ex);
            return new Dictionary<string, object>();
        }
    }

    public async Task<Dictionary<string, object>> GetTemplateAssignmentStatsAsync(Guid templateId)
    {
        return await GetBySubPathAsync<Dictionary<string, object>>(AnalyticsEndpoint, "assignments/template", templateId) ?? new Dictionary<string, object>();
    }
}