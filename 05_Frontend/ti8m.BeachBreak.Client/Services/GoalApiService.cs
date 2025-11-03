using System.Net.Http.Json;
using System.Text.Json;
using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto;

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

    public async Task<Result<Guid>> AddGoalAsync(Guid assignmentId, AddGoalDto dto)
    {
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync(
                $"{CommandEndpoint}/{assignmentId}/goals",
                dto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();
                return result ?? Result<Guid>.Fail("No response from server", 500);
            }

            var errorMessage = await ExtractErrorMessageAsync(response);
            LogError($"Failed to add goal: {errorMessage}", null);
            return Result<Guid>.Fail(errorMessage, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            LogError("Error adding goal", ex);
            return Result<Guid>.Fail($"Error adding goal: {ex.Message}", 500);
        }
    }

    public async Task<Result> ModifyGoalAsync(Guid assignmentId, Guid goalId, ModifyGoalDto dto)
    {
        try
        {
            var response = await HttpCommandClient.PutAsJsonAsync(
                $"{CommandEndpoint}/{assignmentId}/goals/{goalId}",
                dto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result>();
                return result ?? Result.Success();
            }

            var errorMessage = await ExtractErrorMessageAsync(response);
            LogError($"Failed to modify goal: {errorMessage}", null);
            return Result.Fail(errorMessage, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            LogError("Error modifying goal", ex);
            return Result.Fail($"Error modifying goal: {ex.Message}", 500);
        }
    }

    public async Task<Result> DeleteGoalAsync(Guid assignmentId, Guid goalId)
    {
        try
        {
            var response = await HttpCommandClient.DeleteAsync(
                $"{CommandEndpoint}/{assignmentId}/goals/{goalId}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result>();
                return result ?? Result.Success();
            }

            var errorMessage = await ExtractErrorMessageAsync(response);
            LogError($"Failed to delete goal: {errorMessage}", null);
            return Result.Fail(errorMessage, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            LogError("Error deleting goal", ex);
            return Result.Fail($"Error deleting goal: {ex.Message}", 500);
        }
    }

    public async Task<Result> RatePredecessorGoalAsync(Guid assignmentId, RatePredecessorGoalDto dto)
    {
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync(
                $"{CommandEndpoint}/{assignmentId}/goals/rate-predecessor",
                dto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result>();
                return result ?? Result.Success();
            }

            var errorMessage = await ExtractErrorMessageAsync(response);
            LogError($"Failed to rate predecessor goal: {errorMessage}", null);
            return Result.Fail(errorMessage, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            LogError("Error rating predecessor goal", ex);
            return Result.Fail($"Error rating predecessor goal: {ex.Message}", 500);
        }
    }

    public async Task<Result> ModifyPredecessorGoalRatingAsync(Guid assignmentId, Guid sourceGoalId, ModifyPredecessorGoalRatingDto dto)
    {
        try
        {
            var response = await HttpCommandClient.PutAsJsonAsync(
                $"{CommandEndpoint}/{assignmentId}/goals/ratings/{sourceGoalId}",
                dto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result>();
                return result ?? Result.Success();
            }

            var errorMessage = await ExtractErrorMessageAsync(response);
            LogError($"Failed to modify predecessor goal rating: {errorMessage}", null);
            return Result.Fail(errorMessage, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            LogError("Error modifying predecessor goal rating", ex);
            return Result.Fail($"Error modifying rating: {ex.Message}", 500);
        }
    }

    public async Task<Result<IEnumerable<AvailablePredecessorDto>>> GetAvailablePredecessorsAsync(Guid assignmentId, Guid questionId)
    {
        try
        {
            var response = await HttpQueryClient.GetFromJsonAsync<IEnumerable<AvailablePredecessorDto>>(
                $"{QueryEndpoint}/{assignmentId}/predecessors/{questionId}");

            if (response != null)
            {
                return Result<IEnumerable<AvailablePredecessorDto>>.Success(response);
            }

            return Result<IEnumerable<AvailablePredecessorDto>>.Fail("No predecessors found", 404);
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
