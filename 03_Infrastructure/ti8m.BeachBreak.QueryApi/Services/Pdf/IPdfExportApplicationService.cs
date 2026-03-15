using ti8m.BeachBreak.Application.Query.Queries;
using Language = ti8m.BeachBreak.Core.Domain.QuestionConfiguration.Language;

namespace ti8m.BeachBreak.QueryApi.Services.Pdf;

public interface IPdfExportApplicationService
{
    Task<Result<PdfFileResult>> ExportSingleAsync(Guid userId, Guid assignmentId, Language language, CancellationToken ct);
    Task<Result<PdfFileResult>> ExportBulkAsync(Guid userId, IReadOnlyList<Guid> assignmentIds, Language language, CancellationToken ct);
}
