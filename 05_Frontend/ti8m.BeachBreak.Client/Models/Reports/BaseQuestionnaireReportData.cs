using ti8m.BeachBreak.Client.Services;
using ti8m.BeachBreak.Client.Services.Exports;

namespace ti8m.BeachBreak.Client.Models.Reports;

/// <summary>
/// Base class for all questionnaire report data types.
/// Provides common properties and functionality to eliminate duplication across report types.
/// </summary>
public abstract class BaseQuestionnaireReportData : IQuestionnaireReportData
{
    /// <summary>
    /// Date when the report was generated.
    /// </summary>
    public DateTime GeneratedDate { get; set; }

    /// <summary>
    /// Total number of assignments in the report.
    /// </summary>
    public int TotalAssignments { get; set; }

    /// <summary>
    /// Number of completed assignments.
    /// </summary>
    public int CompletedAssignments { get; set; }

    /// <summary>
    /// Number of pending assignments.
    /// </summary>
    public int PendingAssignments { get; set; }

    /// <summary>
    /// Number of overdue assignments.
    /// </summary>
    public int OverdueAssignments { get; set; }

    /// <summary>
    /// Calculates completion rate as a percentage (0-100) with 1 decimal place.
    /// Uses the centralized calculation logic from QuestionnaireMetricsCalculator.
    /// </summary>
    public double CompletionRate => QuestionnaireMetricsCalculator.CalculateCompletionPercentage(
        CompletedAssignments,
        TotalAssignments);

    /// <summary>
    /// Generates the CSV header section for this report type.
    /// Derived classes should override this to provide report-specific headers.
    /// </summary>
    /// <returns>CSV header content including metadata</returns>
    public abstract string GetCsvHeader();

    /// <summary>
    /// Generates the CSV data rows for detailed information.
    /// Derived classes should override this to provide report-specific data rows.
    /// </summary>
    /// <returns>Enumerable of CSV data rows</returns>
    public abstract IEnumerable<string> GetCsvDetailRows();

    /// <summary>
    /// Helper method to generate common header information shared by all reports.
    /// </summary>
    /// <param name="reportTitle">Title of the report</param>
    /// <returns>Common CSV header lines</returns>
    protected string GetCommonHeaderLines(string reportTitle)
    {
        return $@"{reportTitle}
Generated: {GeneratedDate:yyyy-MM-dd HH:mm:ss}
Total Assignments: {TotalAssignments}
Completed: {CompletedAssignments}
Pending: {PendingAssignments}
Overdue: {OverdueAssignments}
Completion Rate: {CompletionRate:F1}%";
    }
}