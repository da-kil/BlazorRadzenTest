using Marten;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Infrastructure.Marten.Repositories;

public class CategoryAggregateRepository : EventSourcedAggregateRepository, ICategoryAggregateRepository
{
    public CategoryAggregateRepository(IDocumentStore store, ILogger<EventSourcedAggregateRepository> logger) : base(store, logger)
    {
    }
}
