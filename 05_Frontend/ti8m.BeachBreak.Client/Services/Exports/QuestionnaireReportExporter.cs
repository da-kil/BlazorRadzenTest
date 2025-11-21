using Microsoft.JSInterop;
using System.Text;
using ti8m.BeachBreak.Client.Models.Reports;

namespace ti8m.BeachBreak.Client.Services.Exports;

/// <summary>
/// Default implementation of IQuestionnaireReportExporter.
/// Handles CSV generation and browser file downloads for questionnaire reports.
/// </summary>
public class QuestionnaireReportExporter : IQuestionnaireReportExporter
{
    private readonly IJSRuntime _jsRuntime;

    public QuestionnaireReportExporter(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Generates CSV content from report data using the report's own formatting methods.
    /// </summary>
    /// <typeparam name="TReportData">Type of report data implementing IQuestionnaireReportData</typeparam>
    /// <param name="data">Report data to export</param>
    /// <returns>CSV formatted string</returns>
    public Task<string> GenerateCSVAsync<TReportData>(TReportData data) where TReportData : IQuestionnaireReportData
    {
        var sb = new StringBuilder();

        // Add header section (metadata)
        sb.AppendLine(data.GetCsvHeader());
        sb.AppendLine();

        // Add detail rows
        foreach (var row in data.GetCsvDetailRows())
        {
            sb.AppendLine(row);
        }

        return Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Triggers file download in the browser using JavaScript interop.
    /// Creates a data URI and triggers download via browser's download mechanism.
    /// </summary>
    /// <param name="content">File content to download</param>
    /// <param name="fileName">Name for the downloaded file</param>
    /// <param name="contentType">MIME type of the content</param>
    public void DownloadFile(string content, string fileName, string contentType)
    {
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
        var dataUri = $"data:{contentType};base64,{base64}";

        _jsRuntime.InvokeVoidAsync("downloadFile", dataUri, fileName);
    }
}