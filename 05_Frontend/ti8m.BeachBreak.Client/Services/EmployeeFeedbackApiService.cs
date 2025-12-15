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
    private const string BaseEndpoint = "c/api/v1/employee-feedbacks";

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
            var response = await HttpCommandClient.PostAsJsonAsync(BaseEndpoint, dto, JsonOptions);

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
            var response = await HttpCommandClient.PutAsJsonAsync($"{BaseEndpoint}/{id}", dto, JsonOptions);

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
            var url = $"{BaseEndpoint}/{id}";
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
            var url = $"q/api/v1/employee-feedbacks{parameters.ToQueryString()}";
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
            var url = $"q/api/v1/employee-feedbacks/{id}";
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
            var response = await HttpQueryClient.GetAsync($"q/api/v1/employee-feedbacks/employee/{employeeId}/current-year");

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
            var url = "q/api/v1/employee-feedbacks/templates";
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
    /// Gets feedback statistics for an employee.
    /// </summary>
    public async Task<Result<object>> GetFeedbackStatisticsAsync(Guid employeeId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var url = $"q/api/v1/employee-feedbacks/employee/{employeeId}/statistics";
            var queryParams = new List<string>();

            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");

            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var response = await HttpQueryClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // API returns statistics object directly, not wrapped in Result
                var statistics = JsonSerializer.Deserialize<object>(content, JsonOptions);
                return Result<object>.Success(statistics ?? new object());
            }

            // For error responses, API may return problem details
            var errorContent = await response.Content.ReadAsStringAsync();
            return Result<object>.Fail($"HTTP {response.StatusCode}: {response.ReasonPhrase}. {errorContent}");
        }
        catch (Exception ex)
        {
            return Result<object>.Fail($"Error getting statistics: {ex.Message}");
        }
    }
}