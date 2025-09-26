using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Repositories;

public interface IQuestionnaireTemplateRepository : IRepository
{
    Task<QuestionnaireTemplateReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionnaireTemplateReadModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionnaireTemplateReadModel>> GetPublishedAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionnaireTemplateReadModel>> GetDraftAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionnaireTemplateReadModel>> GetArchivedAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionnaireTemplateReadModel>> GetAssignableAsync(CancellationToken cancellationToken = default);
}