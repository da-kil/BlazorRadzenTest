using Marten;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Infrastructure.Marten.Repositories;

/// <summary>
/// Repository for FeedbackTemplate aggregate using Marten event sourcing.
/// Follows the same pattern as EmployeeAggregateRepository.
/// </summary>
public class FeedbackTemplateAggregateRepository : EventSourcedAggregateRepository, IFeedbackTemplateAggregateRepository
{
    public FeedbackTemplateAggregateRepository(IDocumentStore store, ILogger<EventSourcedAggregateRepository> logger) : base(store, logger)
    {
    }
}
