using System.IO.Compression;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.QueryApi.Services.Pdf;

public class BulkPdfExportService : IBulkPdfExportService
{
    private readonly IQuestionnairePdfService pdfService;

    public BulkPdfExportService(IQuestionnairePdfService pdfService)
    {
        this.pdfService = pdfService;
    }

    public async Task<byte[]> GenerateBulkZipAsync(IEnumerable<QuestionnairePdfData> items, CancellationToken ct = default)
    {
        using var memoryStream = new MemoryStream();

        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var item in items)
            {
                ct.ThrowIfCancellationRequested();

                var pdfBytes = await pdfService.GeneratePdfAsync(item, ct);
                var templateName = item.Language == Language.German ? item.Template.NameGerman : item.Template.NameEnglish;
                var rawName = $"{item.Assignment.EmployeeName}_{templateName}_{item.Assignment.FinalizedDate:yyyy-MM-dd}.pdf";
                var fileName = SanitizeFileName(rawName);

                var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(pdfBytes, ct);
            }
        }

        return memoryStream.ToArray();
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c))
                     .Replace(' ', '_');
    }
}
