using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeFeedbackQueries;

/// <summary>
/// Query to get employee feedback with filtering and pagination support.
/// Supports filtering by employee, source type, date range, and user access rights.
/// </summary>
public class GetEmployeeFeedbackQuery : IQuery<Result<List<EmployeeFeedbackReadModel>>>
{
    /// <summary>
    /// Filter by specific employee ID (optional).
    /// If not provided, returns feedback for all employees the user has access to.
    /// </summary>
    public Guid? EmployeeId { get; set; }

    /// <summary>
    /// Filter by feedback source type (optional).
    /// </summary>
    public FeedbackSourceType? SourceType { get; set; }

    /// <summary>
    /// Filter by feedback date range - start date (optional).
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter by feedback date range - end date (optional).
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Filter by provider name (partial match, optional).
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// Filter by project name (for project colleague feedback, optional).
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Include deleted feedback in results (default: false).
    /// Only HR+ roles should have access to deleted feedback.
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// Filter to only current fiscal year feedback (default: false).
    /// Useful for questionnaire review integration.
    /// </summary>
    public bool CurrentFiscalYearOnly { get; set; } = false;

    /// <summary>
    /// Page number for pagination (default: 1).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of records per page (default: 50, max: 200).
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Sort field (default: FeedbackDate).
    /// </summary>
    public string SortField { get; set; } = "FeedbackDate";

    /// <summary>
    /// Sort direction - true for ascending, false for descending (default: false - newest first).
    /// </summary>
    public bool SortAscending { get; set; } = false;

    public GetEmployeeFeedbackQuery() { }

    public GetEmployeeFeedbackQuery(Guid? employeeId = null)
    {
        EmployeeId = employeeId;
    }

    /// <summary>
    /// Creates a query for current year feedback for questionnaire review integration.
    /// </summary>
    /// <param name="employeeId">Employee to get feedback for</param>
    /// <returns>Query configured for current year feedback</returns>
    public static GetEmployeeFeedbackQuery ForCurrentYearReview(Guid employeeId)
    {
        return new GetEmployeeFeedbackQuery
        {
            EmployeeId = employeeId,
            CurrentFiscalYearOnly = true,
            IncludeDeleted = false,
            PageSize = 100, // Get more records for review display
            SortField = "SourceType", // Group by source type for review
            SortAscending = true
        };
    }

    /// <summary>
    /// Validates the query parameters.
    /// </summary>
    public Result ValidateQuery()
    {
        if (PageNumber < 1)
            return Result.Fail("Page number must be greater than 0", 400);

        if (PageSize < 1 || PageSize > 200)
            return Result.Fail("Page size must be between 1 and 200", 400);

        if (FromDate.HasValue && ToDate.HasValue && FromDate > ToDate)
            return Result.Fail("From date cannot be greater than To date", 400);

        var validSortFields = new[] { "FeedbackDate", "RecordedDate", "EmployeeName", "ProviderName", "SourceType", "AverageRating" };
        if (!validSortFields.Contains(SortField, StringComparer.OrdinalIgnoreCase))
            return Result.Fail($"Invalid sort field. Valid fields: {string.Join(", ", validSortFields)}", 400);

        return Result.Success();
    }
}