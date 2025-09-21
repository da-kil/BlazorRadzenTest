using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class EmployeeQuestionnaireService : BaseApiService, IEmployeeQuestionnaireService
{
    private const string EmployeeQueryEndpoint = "q/api/v1/employees";
    private const string EmployeeCommandEndpoint = "c/api/v1/employees";
    private readonly string currentEmployeeId;

    public EmployeeQuestionnaireService(IHttpClientFactory factory) : base(factory)
    {
        // TODO: Get current employee ID from authentication context
        currentEmployeeId = "b0f388c2-6294-4116-a8b2-eccafa29b3fb";
    }

    public async Task<List<QuestionnaireAssignment>> GetMyAssignmentsAsync()
    {
        return await GetEmployeeResourceAsync<QuestionnaireAssignment>(EmployeeQueryEndpoint, currentEmployeeId, "assignments");
    }

    public async Task<QuestionnaireAssignment?> GetMyAssignmentByIdAsync(Guid assignmentId)
    {
        return await GetEmployeeSubResourceAsync<QuestionnaireAssignment>(EmployeeQueryEndpoint, currentEmployeeId, "assignments", assignmentId);
    }

    public async Task<QuestionnaireResponse?> GetMyResponseAsync(Guid assignmentId)
    {
        return await GetEmployeeSubResourceAsync<QuestionnaireResponse>(EmployeeQueryEndpoint, currentEmployeeId, "responses/assignment", assignmentId);
    }

    public async Task<QuestionnaireResponse> SaveMyResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses)
    {
        var result = await PostEmployeeResourceAsync<Dictionary<Guid, SectionResponse>, QuestionnaireResponse>(EmployeeCommandEndpoint, currentEmployeeId, "responses/assignment", assignmentId, sectionResponses);
        return result ?? throw new Exception("Failed to save response");
    }

    public async Task<QuestionnaireResponse?> SubmitMyResponseAsync(Guid assignmentId)
    {
        return await PostEmployeeActionAsync<QuestionnaireResponse>(EmployeeCommandEndpoint, currentEmployeeId, "responses/assignment", assignmentId, "submit");
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByStatusAsync(AssignmentStatus status)
    {
        var queryString = $"status={status}";
        return await GetAllAsync<QuestionnaireAssignment>($"{EmployeeQueryEndpoint}/{currentEmployeeId}/assignments", queryString);
    }

    public async Task<AssignmentProgress> GetAssignmentProgressAsync(Guid assignmentId)
    {
        try
        {
            var response = await HttpQueryClient.GetFromJsonAsync<AssignmentProgress>($"{EmployeeQueryEndpoint}/{currentEmployeeId}/assignments/{assignmentId}/progress");
            return response ?? new AssignmentProgress { AssignmentId = assignmentId };
        }
        catch (Exception ex)
        {
            LogError($"Error fetching assignment progress {assignmentId}", ex);
            return new AssignmentProgress { AssignmentId = assignmentId };
        }
    }

    public async Task<List<AssignmentProgress>> GetAllAssignmentProgressAsync()
    {
        return await GetEmployeeResourceAsync<AssignmentProgress>(EmployeeQueryEndpoint, currentEmployeeId, "assignments/progress");
    }
}