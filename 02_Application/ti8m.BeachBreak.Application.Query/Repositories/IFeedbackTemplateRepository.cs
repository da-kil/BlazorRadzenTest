using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Application.Query.Repositories;

/// <summary>
/// Repository interface for querying FeedbackTemplateReadModel projections.
/// </summary>
public interface IFeedbackTemplateRepository : IRepository
{
    Task<FeedbackTemplateReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FeedbackTemplateReadModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<FeedbackTemplateReadModel>> GetBySourceTypeAsync(FeedbackSourceType sourceType, CancellationToken cancellationToken = default);
}
