namespace ti8m.BeachBreak.QueryApi.Services.Pdf;

public interface IQuestionnairePdfService
{
    Task<byte[]> GeneratePdfAsync(QuestionnairePdfData data, CancellationToken ct = default);
}
