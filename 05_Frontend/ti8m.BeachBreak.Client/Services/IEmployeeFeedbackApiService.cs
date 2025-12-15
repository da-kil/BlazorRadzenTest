using ti8m.BeachBreak.Client.Models.Dto;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service interface for employee feedback API operations.
/// Handles communication with both Command and Query APIs.
/// </summary>
public interface IEmployeeFeedbackApiService
{
    /// <summary>
    /// Records new employee feedback.
    /// </summary>
    /// <param name="dto">Feedback data to record</param>
    /// <returns>ID of the created feedback record</returns>
    Task<Result<Guid>> RecordFeedbackAsync(RecordEmployeeFeedbackDto dto);

    /// <summary>
    /// Updates existing employee feedback.
    /// </summary>
    /// <param name="id">Feedback ID to update</param>
    /// <param name="dto">Updated feedback data</param>
    /// <returns>Success/failure result</returns>
    Task<Result> UpdateFeedbackAsync(Guid id, UpdateEmployeeFeedbackDto dto);

    /// <summary>
    /// Deletes employee feedback.
    /// </summary>
    /// <param name="id">Feedback ID to delete</param>
    /// <param name="deleteReason">Optional reason for deletion</param>
    /// <returns>Success/failure result</returns>
    Task<Result> DeleteFeedbackAsync(Guid id, string? deleteReason = null);

    /// <summary>
    /// Gets employee feedback with filtering and pagination.
    /// </summary>
    /// <param name="parameters">Query parameters for filtering</param>
    /// <returns>List of feedback summaries</returns>
    Task<Result<List<EmployeeFeedbackSummaryDto>>> GetEmployeeFeedbackAsync(FeedbackQueryParams parameters);

    /// <summary>
    /// Gets a specific feedback record by ID.
    /// </summary>
    /// <param name="id">Feedback ID</param>
    /// <param name="includeDeleted">Whether to include deleted feedback</param>
    /// <returns>Detailed feedback information</returns>
    Task<Result<EmployeeFeedbackSummaryDto>> GetFeedbackByIdAsync(Guid id, bool includeDeleted = false);

    /// <summary>
    /// Gets current fiscal year feedback for a specific employee.
    /// Used for questionnaire review integration.
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Current year feedback grouped by source type</returns>
    Task<Result<Dictionary<int, List<EmployeeFeedbackSummaryDto>>>> GetCurrentYearFeedbackAsync(Guid employeeId);

    /// <summary>
    /// Gets available feedback templates and criteria.
    /// </summary>
    /// <param name="sourceType">Filter by specific source type</param>
    /// <returns>Available templates and criteria</returns>
    Task<Result<FeedbackTemplatesResponse>> GetFeedbackTemplatesAsync(int? sourceType = null);

    /// <summary>
    /// Gets feedback statistics for an employee.
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="fromDate">Statistics from date</param>
    /// <param name="toDate">Statistics to date</param>
    /// <returns>Aggregated feedback statistics</returns>
    Task<Result<object>> GetFeedbackStatisticsAsync(Guid employeeId, DateTime? fromDate = null, DateTime? toDate = null);
}

/// <summary>
/// Client-side DTO for recording employee feedback.
/// </summary>
public class RecordEmployeeFeedbackDto
{
    public Guid EmployeeId { get; set; }
    public int SourceType { get; set; }
    public FeedbackProviderInfoDto ProviderInfo { get; set; } = null!;
    public DateTime FeedbackDate { get; set; }
    public ConfigurableFeedbackDataDto FeedbackData { get; set; } = null!;
}

// UpdateEmployeeFeedbackDto is now in Models.Dto namespace

// FeedbackProviderInfoDto is now in Models.Dto namespace

// ConfigurableFeedbackDataDto and FeedbackRatingDto are now in Models.Dto namespace

/// <summary>
/// Response containing available feedback templates and criteria.
/// </summary>
public class FeedbackTemplatesResponse
{
    public Dictionary<int, object> DefaultTemplates { get; set; } = new();
    public List<object> AvailableCriteria { get; set; } = new();
    public List<object> StandardTextSections { get; set; } = new();
    public List<SourceTypeOption> SourceTypeOptions { get; set; } = new();
}

/// <summary>
/// Source type option for UI display.
/// </summary>
public class SourceTypeOption
{
    public int Value { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool RequiresProjectContext { get; set; }
    public bool RequiresProviderRole { get; set; }
}