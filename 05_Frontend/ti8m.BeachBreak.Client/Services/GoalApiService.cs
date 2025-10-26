using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
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
    private const string CommandEndpoint = "c/api/v1/assignments";
    private readonly IJSRuntime _jsRuntime;

    public GoalApiService(IHttpClientFactory factory, IJSRuntime jsRuntime) : base(factory)
    {
        _jsRuntime = jsRuntime;
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
            var url = $"{QueryEndpoint}/{assignmentId}/goals/{questionId}";
            await _jsRuntime.InvokeVoidAsync("console.log", $"[GoalApiService] GET {url}");

            var response = await HttpQueryClient.GetFromJsonAsync<GoalQuestionDataDto>(url);

            await _jsRuntime.InvokeVoidAsync("console.log", $"[GoalApiService] Response received - IsNull: {response == null}");

            if (response != null)
            {
                await _jsRuntime.InvokeVoidAsync("console.log", $"[GoalApiService] Response.Goals.Count: {response.Goals?.Count ?? 0}");
                await _jsRuntime.InvokeVoidAsync("console.log", $"[GoalApiService] Response.QuestionId: {response.QuestionId}");
                await _jsRuntime.InvokeVoidAsync("console.log", $"[GoalApiService] Response.PredecessorAssignmentId: {response.PredecessorAssignmentId}");

                if (response.Goals != null && response.Goals.Any())
                {
                    foreach (var goal in response.Goals)
                    {
                        await _jsRuntime.InvokeVoidAsync("console.log", $"[GoalApiService] Goal: Id={goal.Id}, Objective={goal.ObjectiveDescription}, AddedByRole={goal.AddedByRole}");
                    }
                }
                else
                {
                    await _jsRuntime.InvokeVoidAsync("console.log", "[GoalApiService] Response.Goals is null or empty");
                }

                return Result<GoalQuestionDataDto>.Success(response);
            }

            await _jsRuntime.InvokeVoidAsync("console.log", "[GoalApiService] Response is null - returning Fail");
            return Result<GoalQuestionDataDto>.Fail("No goal data found", 404);
        }
        catch (Exception ex)
        {
            await _jsRuntime.InvokeVoidAsync("console.error", $"[GoalApiService] EXCEPTION: {ex.Message}");
            await _jsRuntime.InvokeVoidAsync("console.error", $"[GoalApiService] StackTrace: {ex.StackTrace}");
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
