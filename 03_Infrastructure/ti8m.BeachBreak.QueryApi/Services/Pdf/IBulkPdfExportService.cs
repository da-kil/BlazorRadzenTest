namespace ti8m.BeachBreak.QueryApi.Services.Pdf;

public interface IBulkPdfExportService
{
    Task<byte[]> GenerateBulkZipAsync(IEnumerable<QuestionnairePdfData> items, CancellationToken ct = default);
}
