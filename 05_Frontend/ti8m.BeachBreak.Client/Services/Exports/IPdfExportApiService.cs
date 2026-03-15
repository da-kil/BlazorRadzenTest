using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services.Exports;

public interface IPdfExportApiService
{
    Task<bool> ExportSinglePdfAsync(Guid assignmentId, string suggestedFileName, PdfLanguage language = PdfLanguage.English, CancellationToken ct = default);
    Task<bool> ExportBulkPdfAsync(IEnumerable<Guid> assignmentIds, PdfLanguage language = PdfLanguage.English, CancellationToken ct = default);
}
