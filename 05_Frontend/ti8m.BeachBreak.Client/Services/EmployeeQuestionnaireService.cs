using Microsoft.JSInterop;
using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class EmployeeQuestionnaireService : BaseApiService, IEmployeeQuestionnaireService
{
    private const string EmployeeQueryEndpoint = "q/api/v1/employees";
    private const string EmployeeCommandEndpoint = "c/api/v1/employees";
    private readonly IJSRuntime _jsRuntime;

    public EmployeeQuestionnaireService(IHttpClientFactory factory, IJSRuntime jsRuntime) : base(factory)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<List<QuestionnaireAssignment>> GetMyAssignmentsAsync()
    {
        // Call the secure "me" endpoint - backend uses UserContext to get employee ID
        return await GetAllAsync<QuestionnaireAssignment>($"{EmployeeQueryEndpoint}/me/assignments");
    }

    public async Task<QuestionnaireAssignment?> GetMyAssignmentByIdAsync(Guid assignmentId)
    {
        // Use "me" endpoint - backend resolves employee ID from UserContext
        try
        {
            var endpoint = $"{EmployeeQueryEndpoint}/me/assignments/{assignmentId}";
            var result = await HttpQueryClient.GetFromJsonAsync<QuestionnaireAssignment>(endpoint);
            return result;
        }
        catch (Exception ex)
        {
            LogError($"Error fetching assignment {assignmentId}", ex);
            return null;
        }
    }

    public async Task<QuestionnaireResponse?> GetMyResponseAsync(Guid assignmentId)
    {
        // Use "me" endpoint - backend resolves employee ID from UserContext
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<QuestionnaireResponse>($"{EmployeeQueryEndpoint}/me/responses/assignment/{assignmentId}");
        }
        catch (Exception ex)
        {
            LogError($"Error fetching response for assignment {assignmentId}", ex);
            return null;
        }
    }

    public async Task<QuestionnaireResponse> SaveMyResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses)
    {
        // Use "me" endpoint - backend resolves employee ID from UserContext for security
        try
        {
            // Convert section-based responses to question-based DTO format expected by backend
            var dto = QuestionnaireResponseConverter.ConvertToSaveQuestionnaireResponseDto(sectionResponses, templateId: null);

            var response = await HttpCommandClient.PostAsJsonAsync($"{EmployeeCommandEndpoint}/me/responses/assignment/{assignmentId}", dto, JsonOptions);
            response.EnsureSuccessStatusCode();

            // Backend returns Result<Guid>, not just Guid
            var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();
            if (result == null || !result.Succeeded)
            {
                throw new Exception(result?.Message ?? "Failed to save response");
            }

            var responseId = result.Payload;
            return new QuestionnaireResponse { Id = responseId, AssignmentId = assignmentId, SectionResponses = sectionResponses };
        }
        catch (Exception ex)
        {
            LogError($"Error saving response for assignment {assignmentId}", ex);
            throw;
        }
    }

    /// <summary>
    /// Saves the currently authenticated employee's response with templateId optimization.
    /// When templateId is provided, the backend skips assignment lookup for better performance.
    /// </summary>
    public async Task<QuestionnaireResponse> SaveMyResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses, Guid? templateId)
    {
        // Use "me" endpoint - backend resolves employee ID from UserContext for security
        try
        {
            // Convert section-based responses to question-based DTO format expected by backend
            var dto = QuestionnaireResponseConverter.ConvertToSaveQuestionnaireResponseDto(sectionResponses, templateId);

            var response = await HttpCommandClient.PostAsJsonAsync($"{EmployeeCommandEndpoint}/me/responses/assignment/{assignmentId}", dto, JsonOptions);
            response.EnsureSuccessStatusCode();

            // Backend returns Result<Guid>, not just Guid
            var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();
            if (result == null || !result.Succeeded)
            {
                throw new Exception(result?.Message ?? "Failed to save response");
            }

            var responseId = result.Payload;
            return new QuestionnaireResponse { Id = responseId, AssignmentId = assignmentId, SectionResponses = sectionResponses };
        }
        catch (Exception ex)
        {
            LogError($"Error saving response for assignment {assignmentId}", ex);
            throw;
        }
    }

    public async Task<QuestionnaireResponse?> SubmitMyResponseAsync(Guid assignmentId)
    {
        // Use "me" endpoint - backend resolves employee ID from UserContext for security
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync($"{EmployeeCommandEndpoint}/me/responses/assignment/{assignmentId}/submit", new { }, JsonOptions);
            response.EnsureSuccessStatusCode();

            // Backend returns Result (void), not QuestionnaireResponse
            // So we reload the response from the query API after successful submission
            return await GetMyResponseAsync(assignmentId);
        }
        catch (Exception ex)
        {
            LogError($"Error submitting response for assignment {assignmentId}", ex);
            return null;
        }
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByWorkflowStateAsync(WorkflowState workflowState)
    {
        // Use "me" endpoint with workflow state filter - backend resolves employee ID from UserContext
        var queryString = $"workflowState={workflowState}";
        return await GetAllAsync<QuestionnaireAssignment>($"{EmployeeQueryEndpoint}/me/assignments", queryString);
    }

    public async Task<AssignmentProgress> GetAssignmentProgressAsync(Guid assignmentId)
    {
        try
        {
            // Use "me" endpoint - backend resolves employee ID from UserContext
            var response = await HttpQueryClient.GetFromJsonAsync<AssignmentProgress>($"{EmployeeQueryEndpoint}/me/assignments/{assignmentId}/progress");
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
        // Use "me" endpoint - backend resolves employee ID from UserContext
        return await GetAllAsync<AssignmentProgress>($"{EmployeeQueryEndpoint}/me/assignments/progress");
    }
}