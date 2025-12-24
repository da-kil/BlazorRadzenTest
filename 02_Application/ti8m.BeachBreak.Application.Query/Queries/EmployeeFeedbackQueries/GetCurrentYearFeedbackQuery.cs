using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Core;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeFeedbackQueries;

/// <summary>
/// Query to get current fiscal year feedback for a specific employee.
/// Specifically designed for questionnaire review integration.
/// </summary>
public class GetCurrentYearFeedbackQuery : IQuery<Result<List<EmployeeFeedbackReadModel>>>
{
    /// <summary>
    /// ID of the employee to get feedback for.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Whether to group results by source type for better organization.
    /// Default: true for review display.
    /// </summary>
    public bool GroupBySourceType { get; set; } = true;

    /// <summary>
    /// Include summary statistics in the response.
    /// Default: true for review context.
    /// </summary>
    public bool IncludeSummaryStats { get; set; } = true;

    public GetCurrentYearFeedbackQuery() { }

    public GetCurrentYearFeedbackQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }
}