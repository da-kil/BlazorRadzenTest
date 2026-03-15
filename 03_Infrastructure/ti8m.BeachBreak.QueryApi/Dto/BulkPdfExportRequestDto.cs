using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.QueryApi.Dto;

public class BulkPdfExportRequestDto
{
    public List<Guid> AssignmentIds { get; set; } = new();
    public Language Language { get; set; } = Language.English;
}
