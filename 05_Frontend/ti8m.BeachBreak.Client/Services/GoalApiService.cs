using System.Net.Http.Json;
using System.Text.Json;
using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto;
using ti8m.BeachBreak.Client.Models.Dto.Shared;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// API service for goal question operations.
/// Handles all goal-related API calls (add, modify, rate, link predecessors).
/// </summary>
public class GoalApiService : BaseApiService, IGoalApiService
{
    private const string QueryEndpoint = "q/api/v1/assignments";
    private const string EmployeeQueryEndpoint = "q/api/v1/employees/me/assignments";
    private const string CommandEndpoint = "c/api/v1/assignments";
    private readonly IAuthService authService;

    public GoalApiService(IHttpClientFactory factory, IAuthService authService) : base(factory)
    {
        this.authService = authService;
    }

    public async Task<Result> LinkPredecessorAsync(Guid assignmentId, LinkPredecessorQuestionnaireDto dto)
    {
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync(
                $"{CommandEndpoint}/{assignmentId}/goals/link-predecessor",
                dto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result>();
                return result ?? Result.Success();
            }

            var errorMessage = await ExtractErrorMessageAsync(response);
            LogError($"Failed to link predecessor: {errorMessage}", null);
            return Result.Fail(errorMessage, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            LogError("Error linking predecessor questionnaire", ex);
            return Result.Fail($"Error linking predecessor: {ex.Message}", 500);
        }
    }

    public async Task<Result<IEnumerable<AvailablePredecessorDto>>> GetAvailablePredecessorsAsync(Guid assignmentId)
    {
        try
        {
            var response = await HttpQueryClient.GetAsync(
                $"{QueryEndpoint}/{assignmentId}/predecessors");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<IEnumerable<AvailablePredecessorDto>>();
                if (data != null)
                {
                    return Result<IEnumerable<AvailablePredecessorDto>>.Success(data);
                }
                return Result<IEnumerable<AvailablePredecessorDto>>.Fail("No predecessors found", 404);
            }

            var errorMessage = await ExtractErrorMessageAsync(response);
            LogError($"Failed to fetch predecessors: {errorMessage}", null);
            return Result<IEnumerable<AvailablePredecessorDto>>.Fail(errorMessage, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            LogError("Error fetching available predecessors", ex);
            return Result<IEnumerable<AvailablePredecessorDto>>.Fail($"Error fetching predecessors: {ex.Message}", 500);
        }
    }

    public async Task<Result<GoalQuestionDataDto>> GetGoalQuestionDataAsync(Guid assignmentId, Guid questionId)
    {
        try
        {
            // Determine which endpoint to use based on user role
            var userRole = await authService.GetMyRoleAsync();
            var isManager = userRole?.ApplicationRole is ApplicationRole.TeamLead
                or ApplicationRole.HR
                or ApplicationRole.HRLead
                or ApplicationRole.Admin;

            string endpoint;
            if (isManager)
            {
                // Managers use the general assignments endpoint
                endpoint = $"{QueryEndpoint}/{assignmentId}/goals/{questionId}";
            }
            else
            {
                // Employees use the "me" endpoint which validates ownership
                endpoint = $"{EmployeeQueryEndpoint}/{assignmentId}/goals/{questionId}";
            }

            var response = await HttpQueryClient.GetFromJsonAsync<GoalQuestionDataDto>(endpoint);

            if (response != null)
            {
                return Result<GoalQuestionDataDto>.Success(response);
            }

            return Result<GoalQuestionDataDto>.Fail("No goal data found", 404);
        }
        catch (Exception ex)
        {
            LogError("Error fetching goal question data", ex);
            return Result<GoalQuestionDataDto>.Fail($"Error fetching goal data: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Extracts detailed error message from ProblemDetails or falls back to response content.
    /// </summary>
    private async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();

            // Try to parse as ProblemDetails
            var problemDetails = JsonSerializer.Deserialize<JsonElement>(content);

            // Check if it has a "detail" property (ProblemDetails format)
            if (problemDetails.TryGetProperty("detail", out var detailProperty))
            {
                var detail = detailProperty.GetString();
                if (!string.IsNullOrWhiteSpace(detail))
                {
                    return detail;
                }
            }

            // Fallback to raw content if not empty
            if (!string.IsNullOrWhiteSpace(content))
            {
                return content;
            }
        }
        catch
        {
            // If parsing fails, fall through to reason phrase
        }

        // Final fallback to HTTP reason phrase
        return response.ReasonPhrase ?? "Unknown error";
    }
}
