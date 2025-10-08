using Marten;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate;

namespace ti8m.BeachBreak.Infrastructure.Marten.Repositories;

public class QuestionnaireResponseAggregateRepository : EventSourcedAggregateRepository, IQuestionnaireResponseAggregateRepository
{
    public QuestionnaireResponseAggregateRepository(IDocumentStore store, ILogger<EventSourcedAggregateRepository> logger) : base(store, logger)
    {
    }

    public async Task<QuestionnaireResponse?> FindByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        using var session = store.LightweightSession();

        // Query the read model to find the aggregate ID
        var readModel = await session.Query<QuestionnaireResponseReadModel>()
            .Where(r => r.AssignmentId == assignmentId)
            .FirstOrDefaultAsync(cancellationToken);

        if (readModel == null)
        {
            return null;
        }

        // Load the aggregate from the event stream using the actual aggregate ID
        return await LoadAsync<QuestionnaireResponse>(readModel.Id, cancellationToken: cancellationToken);
    }
}
