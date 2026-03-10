using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Services;

namespace ti8m.BeachBreak.Infrastructure.Marten.Services;

public class QuestionnaireAssignmentService : IQuestionnaireAssignmentService
{
    private readonly IDocumentStore store;

    public QuestionnaireAssignmentService(IDocumentStore store)
    {
        this.store = store;
    }

    public async Task<bool> HasActiveAssignmentsAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync(token: cancellationToken);
        return await session.Query<QuestionnaireAssignmentReadModel>()
            .AnyAsync(a => a.TemplateId == templateId
                        && !a.IsWithdrawn
                        && a.WorkflowState != WorkflowState.Finalized,
                      cancellationToken);
    }

    public async Task<int> GetActiveAssignmentCountAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync(token: cancellationToken);
        return await session.Query<QuestionnaireAssignmentReadModel>()
            .CountAsync(a => a.TemplateId == templateId
                          && !a.IsWithdrawn
                          && a.WorkflowState != WorkflowState.Finalized,
                        cancellationToken);
    }

    public async Task<bool> HasAnyAssignmentsAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync(token: cancellationToken);
        return await session.Query<QuestionnaireAssignmentReadModel>()
            .AnyAsync(a => a.TemplateId == templateId, cancellationToken);
    }
}