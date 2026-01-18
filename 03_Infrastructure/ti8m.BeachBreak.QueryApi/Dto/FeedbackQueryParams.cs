using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.QueryApi.Serialization;

namespace ti8m.BeachBreak.QueryApi.Dto;

/// <summary>
/// Query parameters for filtering employee feedback.
/// </summary>
[RegisterJsonSerialization(typeof(QueryApiJsonSerializerContext))]
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
}