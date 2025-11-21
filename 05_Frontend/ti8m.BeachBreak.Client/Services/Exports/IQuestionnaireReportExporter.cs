using ti8m.BeachBreak.Client.Models.Reports;

namespace ti8m.BeachBreak.Client.Services.Exports;

/// <summary>
/// Interface for exporting questionnaire reports to various formats.
/// Provides common export operations across different report types.
/// </summary>
public interface IQuestionnaireReportExporter
{
    /// <summary>
    /// Generates CSV content from report data.
    /// </summary>
    /// <typeparam name="TReportData">Type of report data implementing IQuestionnaireReportData</typeparam>
    /// <param name="data">Report data to export</param>
    /// <returns>CSV formatted string</returns>
    Task<string> GenerateCSVAsync<TReportData>(TReportData data) where TReportData : IQuestionnaireReportData;

    /// <summary>
    /// Triggers file download in the browser.
    /// </summary>
    /// <param name="content">File content to download</param>
    /// <param name="fileName">Name for the downloaded file</param>
    /// <param name="contentType">MIME type of the content</param>
    void DownloadFile(string content, string fileName, string contentType);
}

/// <summary>
/// Base interface for all questionnaire report data types.
/// Defines common properties and operations for CSV export.
/// </summary>
public interface IQuestionnaireReportData
{
    /// <summary>
    /// Date when the report was generated.
    /// </summary>
    DateTime GeneratedDate { get; set; }

    /// <summary>
    /// Total number of assignments in the report.
    /// </summary>
    int TotalAssignments { get; set; }

    /// <summary>
    /// Number of completed assignments.
    /// </summary>
    int CompletedAssignments { get; set; }

    /// <summary>
    /// Number of pending assignments.
    /// </summary>
    int PendingAssignments { get; set; }

    /// <summary>
    /// Number of overdue assignments.
    /// </summary>
    int OverdueAssignments { get; set; }

    /// <summary>
    /// Generates the CSV header section for this report type.
    /// </summary>
    /// <returns>CSV header content including metadata</returns>
    string GetCsvHeader();

    /// <summary>
    /// Generates the CSV data rows for detailed information.
    /// </summary>
    /// <returns>Enumerable of CSV data rows</returns>
    IEnumerable<string> GetCsvDetailRows();
}