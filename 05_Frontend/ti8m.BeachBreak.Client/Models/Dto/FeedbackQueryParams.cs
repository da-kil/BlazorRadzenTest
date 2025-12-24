namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// Client-side query parameters for filtering employee feedback.
/// Matches the QueryApi DTO for type-safe communication.
/// </summary>
public class FeedbackQueryParams
{
    /// <summary>
    /// Filter by specific employee ID.
    /// </summary>
    public Guid? EmployeeId { get; set; }

    /// <summary>
    /// Filter by feedback source type (0=Customer, 1=Peer, 2=ProjectColleague).
    /// </summary>
    public int? SourceType { get; set; }

    /// <summary>
    /// Filter by feedback date range - start date.
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter by feedback date range - end date.
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Filter by provider name (partial match).
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// Filter by project name (for project colleague feedback).
    /// </summary>
    public string? ProjectName { get; set; }

    /// <summary>
    /// Include deleted feedback in results (default: false).
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// Filter to only current fiscal year feedback (default: false).
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
    /// Sort direction - true for ascending, false for descending.
    /// </summary>
    public bool SortAscending { get; set; } = false;

    /// <summary>
    /// Builds query string for HTTP requests.
    /// </summary>
    public string ToQueryString()
    {
        var parameters = new List<string>();

        if (EmployeeId.HasValue)
            parameters.Add($"employeeId={EmployeeId.Value}");

        if (SourceType.HasValue)
            parameters.Add($"sourceType={SourceType.Value}");

        if (FromDate.HasValue)
            parameters.Add($"fromDate={FromDate.Value:yyyy-MM-dd}");

        if (ToDate.HasValue)
            parameters.Add($"toDate={ToDate.Value:yyyy-MM-dd}");

        if (!string.IsNullOrWhiteSpace(ProviderName))
            parameters.Add($"providerName={Uri.EscapeDataString(ProviderName)}");

        if (!string.IsNullOrWhiteSpace(ProjectName))
            parameters.Add($"projectName={Uri.EscapeDataString(ProjectName)}");

        if (IncludeDeleted)
            parameters.Add("includeDeleted=true");

        if (CurrentFiscalYearOnly)
            parameters.Add("currentFiscalYearOnly=true");

        if (PageNumber != 1)
            parameters.Add($"pageNumber={PageNumber}");

        if (PageSize != 50)
            parameters.Add($"pageSize={PageSize}");

        if (SortField != "FeedbackDate")
            parameters.Add($"sortField={SortField}");

        if (SortAscending)
            parameters.Add("sortAscending=true");

        return parameters.Any() ? "?" + string.Join("&", parameters) : string.Empty;
    }

    /// <summary>
    /// Creates default parameters for current year review.
    /// </summary>
    public static FeedbackQueryParams ForCurrentYearReview(Guid employeeId)
    {
        return new FeedbackQueryParams
        {
            EmployeeId = employeeId,
            CurrentFiscalYearOnly = true,
            IncludeDeleted = false,
            PageSize = 100,
            SortField = "SourceType",
            SortAscending = true
        };
    }
}