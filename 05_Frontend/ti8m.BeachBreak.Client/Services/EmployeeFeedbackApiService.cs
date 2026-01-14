using System.Text.Json;
using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models.Dto;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// HTTP client service for employee feedback API operations.
/// Handles communication with both Command and Query APIs.
/// </summary>
public class EmployeeFeedbackApiService : BaseApiService, IEmployeeFeedbackApiService
{
    private const string CommandEndpoint = "c/api/v1/employee-feedbacks";
    private const string QueryEndpoint = "q/api/v1/employee-feedbacks";

    public EmployeeFeedbackApiService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {
    }

    /// <summary>
    /// Records new employee feedback.
    /// </summary>
    public async Task<Result<Guid>> RecordFeedbackAsync(RecordEmployeeFeedbackDto dto)
    {
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync(CommandEndpoint, dto, JsonOptions);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Result<Guid>>(content, JsonOptions);
                return result ?? Result<Guid>.Fail("Failed to deserialize response");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResult = JsonSerializer.Deserialize<Result<Guid>>(errorContent, JsonOptions);
            return errorResult ?? Result<Guid>.Fail($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Error recording feedback: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates existing employee feedback.
    /// </summary>
    public async Task<Result> UpdateFeedbackAsync(Guid id, UpdateEmployeeFeedbackDto dto)
    {
        try
        {
            var response = await HttpCommandClient.PutAsJsonAsync($"{CommandEndpoint}/{id}", dto, JsonOptions);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Result>(content, JsonOptions);
                return result ?? Result.Success();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResult = JsonSerializer.Deserialize<Result>(errorContent, JsonOptions);
            return errorResult ?? Result.Fail($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error updating feedback: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes employee feedback.
    /// </summary>
    public async Task<Result> DeleteFeedbackAsync(Guid id, string? deleteReason = null)
    {
        try
        {
            var url = $"{CommandEndpoint}/{id}";
            if (!string.IsNullOrWhiteSpace(deleteReason))
            {
                url += $"?deleteReason={Uri.EscapeDataString(deleteReason)}";
            }

            var response = await HttpCommandClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Result>(content, JsonOptions);
                return result ?? Result.Success();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResult = JsonSerializer.Deserialize<Result>(errorContent, JsonOptions);
            return errorResult ?? Result.Fail($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return Result.Fail($"Error deleting feedback: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets employee feedback with filtering and pagination.
    /// </summary>
    public async Task<Result<List<EmployeeFeedbackSummaryDto>>> GetEmployeeFeedbackAsync(FeedbackQueryParams parameters)
    {
        try
        {
            var url = $"{QueryEndpoint}{parameters.ToQueryString()}";
            var response = await HttpQueryClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // API returns List<EmployeeFeedbackSummaryDto> directly, not wrapped in Result
                // Client DTO has extra properties but JSON deserialization will handle the mapping
                var feedbackList = JsonSerializer.Deserialize<List<EmployeeFeedbackSummaryDto>>(content, JsonOptions);
                return Result<List<EmployeeFeedbackSummaryDto>>.Success(feedbackList ?? new List<EmployeeFeedbackSummaryDto>());
            }

            // For error responses, API may return problem details or Result format
            var errorContent = await response.Content.ReadAsStringAsync();
            return Result<List<EmployeeFeedbackSummaryDto>>.Fail($"HTTP {response.StatusCode}: {response.ReasonPhrase}. {errorContent}");
        }
        catch (Exception ex)
        {
            return Result<List<EmployeeFeedbackSummaryDto>>.Fail($"Error getting feedback: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a specific feedback record by ID.
    /// </summary>
    public async Task<Result<EmployeeFeedbackSummaryDto>> GetFeedbackByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            var url = $"{QueryEndpoint}/{id}";
            if (includeDeleted)
            {
                url += "?includeDeleted=true";
            }

            var response = await HttpQueryClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // API returns single EmployeeFeedbackSummaryDto directly via CreateResponse
                // Client DTO has extra properties but JSON deserialization will handle the mapping
                var feedbackDto = JsonSerializer.Deserialize<EmployeeFeedbackSummaryDto>(content, JsonOptions);
                return Result<EmployeeFeedbackSummaryDto>.Success(feedbackDto ?? new EmployeeFeedbackSummaryDto());
            }

            // For error responses, API may return problem details
            var errorContent = await response.Content.ReadAsStringAsync();
            return Result<EmployeeFeedbackSummaryDto>.Fail($"HTTP {response.StatusCode}: {response.ReasonPhrase}. {errorContent}");
        }
        catch (Exception ex)
        {
            return Result<EmployeeFeedbackSummaryDto>.Fail($"Error getting feedback: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets current fiscal year feedback for a specific employee.
    /// </summary>
    public async Task<Result<Dictionary<int, List<EmployeeFeedbackSummaryDto>>>> GetCurrentYearFeedbackAsync(Guid employeeId)
    {
        try
        {
            var response = await HttpQueryClient.GetAsync($"{QueryEndpoint}/employee/{employeeId}/current-year");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // API returns Dictionary<int, List<EmployeeFeedbackSummaryDto>> directly, not wrapped in Result
                // Client DTO has extra properties but JSON deserialization will handle the mapping
                var groupedFeedback = JsonSerializer.Deserialize<Dictionary<int, List<EmployeeFeedbackSummaryDto>>>(content, JsonOptions);
                return Result<Dictionary<int, List<EmployeeFeedbackSummaryDto>>>.Success(groupedFeedback ?? new Dictionary<int, List<EmployeeFeedbackSummaryDto>>());
            }

            // For error responses, API may return problem details
            var errorContent = await response.Content.ReadAsStringAsync();
            return Result<Dictionary<int, List<EmployeeFeedbackSummaryDto>>>.Fail($"HTTP {response.StatusCode}: {response.ReasonPhrase}. {errorContent}");
        }
        catch (Exception ex)
        {
            return Result<Dictionary<int, List<EmployeeFeedbackSummaryDto>>>.Fail($"Error getting current year feedback: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets available feedback templates and criteria.
    /// </summary>
    public async Task<Result<FeedbackTemplatesResponse>> GetFeedbackTemplatesAsync(int? sourceType = null)
    {
        try
        {
            var url = $"{QueryEndpoint}/templates";
            if (sourceType.HasValue)
            {
                url += $"?sourceType={sourceType.Value}";
            }

            var response = await HttpQueryClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Result<FeedbackTemplatesResponse>>(content, JsonOptions);
                return result ?? Result<FeedbackTemplatesResponse>.Fail("Failed to deserialize response");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResult = JsonSerializer.Deserialize<Result<FeedbackTemplatesResponse>>(errorContent, JsonOptions);
            return errorResult ?? Result<FeedbackTemplatesResponse>.Fail($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return Result<FeedbackTemplatesResponse>.Fail($"Error getting templates: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets source type options for feedback recording.
    /// </summary>
    public async Task<Result<FeedbackTemplatesResponse>> GetSourceTypeOptionsAsync()
    {
        try
        {
            var url = $"{QueryEndpoint}/source-types";
            var response = await HttpQueryClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Result<FeedbackTemplatesResponse>>(content, JsonOptions);
                return result ?? Result<FeedbackTemplatesResponse>.Fail("Failed to deserialize response");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResult = JsonSerializer.Deserialize<Result<FeedbackTemplatesResponse>>(errorContent, JsonOptions);
            return errorResult ?? Result<FeedbackTemplatesResponse>.Fail($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return Result<FeedbackTemplatesResponse>.Fail($"Error getting source type options: {ex.Message}");
        }
    }

    // Questionnaire Assignment Feedback Linking

    /// <summary>
    /// Links employee feedback to a questionnaire assignment.
    /// </summary>
    public async Task<bool> LinkFeedbackToAssignmentAsync(Guid assignmentId, LinkEmployeeFeedbackDto dto)
    {
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync(
                $"c/api/v1/assignments/{assignmentId}/feedback/link", dto, JsonOptions);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error linking feedback to assignment {assignmentId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Unlinks employee feedback from a questionnaire assignment.
    /// </summary>
    public async Task<bool> UnlinkFeedbackFromAssignmentAsync(Guid assignmentId, UnlinkEmployeeFeedbackDto dto)
    {
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync(
                $"c/api/v1/assignments/{assignmentId}/feedback/unlink", dto, JsonOptions);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error unlinking feedback from assignment {assignmentId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets all available employee feedback records that can be linked to an assignment.
    /// </summary>
    public async Task<List<LinkedEmployeeFeedbackDto>> GetAvailableFeedbackForAssignmentAsync(Guid assignmentId)
    {
        try
        {
            var response = await HttpQueryClient.GetAsync(
                $"q/api/v1/assignments/{assignmentId}/feedback/available");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to get available feedback for assignment {assignmentId}");
                return new List<LinkedEmployeeFeedbackDto>();
            }

            return await response.Content.ReadFromJsonAsync<List<LinkedEmployeeFeedbackDto>>(JsonOptions)
                ?? new List<LinkedEmployeeFeedbackDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching available feedback for assignment {assignmentId}: {ex.Message}");
            return new List<LinkedEmployeeFeedbackDto>();
        }
    }

    /// <summary>
    /// Gets all linked feedback data for a specific question within an assignment.
    /// </summary>
    public async Task<FeedbackQuestionDataDto?> GetFeedbackQuestionDataAsync(Guid assignmentId, Guid questionId)
    {
        try
        {
            var response = await HttpQueryClient.GetAsync(
                $"q/api/v1/assignments/{assignmentId}/feedback/{questionId}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to get feedback question data for question {questionId}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FeedbackQuestionDataDto>(JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching feedback question data for question {questionId}: {ex.Message}");
            return null;
        }
    }
}