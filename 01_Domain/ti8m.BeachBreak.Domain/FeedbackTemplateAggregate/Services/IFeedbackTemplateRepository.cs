namespace ti8m.BeachBreak.Domain.FeedbackTemplateAggregate.Services;

public interface IFeedbackTemplateRepository
{
    Task<FeedbackTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAsync(FeedbackTemplate template, CancellationToken cancellationToken = default);
}
