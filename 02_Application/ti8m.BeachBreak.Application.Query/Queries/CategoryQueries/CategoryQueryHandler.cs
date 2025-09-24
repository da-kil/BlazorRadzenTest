using Microsoft.Extensions.Logging;
using Marten;
using ti8m.BeachBreak.Domain.CategoryAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.CategoryQueries;

public class CategoryQueryHandler :
    IQueryHandler<CategoryListQuery, Result<IEnumerable<Category>>>,
    IQueryHandler<CategoryQuery, Result<Category>>
{
    private readonly IQuerySession session;
    private readonly ILogger<CategoryQueryHandler> logger;

    public CategoryQueryHandler(IQuerySession session, ILogger<CategoryQueryHandler> logger)
    {
        this.session = session;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<Category>>> HandleAsync(CategoryListQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // For event sourcing, we need to load aggregates from event streams
            // For now, this is a simplified approach - in a real scenario, we'd have projections for read models
            var categories = new List<Category>();

            // This approach loads all category streams - not efficient for large datasets
            // In practice, you'd want read model projections
            var streamIds = await session.Events.QueryAllRawEvents()
                .Where(e => e.StreamKey.StartsWith("Category"))
                .Select(e => e.StreamId)
                .Distinct()
                .ToListAsync(cancellationToken);

            foreach (var streamId in streamIds)
            {
                var category = await session.Events.AggregateStreamAsync<Category>(streamId, token: cancellationToken);
                if (category != null)
                {
                    // Filter by active status if needed
                    if (!query.IncludeInactive && !category.IsActive)
                        continue;

                    categories.Add(category);
                }
            }

            // Order by sort order and name
            var orderedCategories = categories
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name.English)
                .ToList();

            logger.LogInformation("Retrieved {Count} categories", orderedCategories.Count);
            return Result<IEnumerable<Category>>.Success(orderedCategories);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve categories");
            return Result<IEnumerable<Category>>.Fail($"Failed to retrieve categories: {ex.Message}", 500);
        }
    }

    public async Task<Result<Category>> HandleAsync(CategoryQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Load the category from event stream
            var category = await session.Events.AggregateStreamAsync<Category>(query.CategoryId, token: cancellationToken);

            if (category != null)
            {
                logger.LogInformation("Retrieved category with ID {Id}", query.CategoryId);
                return Result<Category>.Success(category);
            }

            logger.LogWarning("Category with ID {Id} not found", query.CategoryId);
            return Result<Category>.Fail($"Category with ID {query.CategoryId} not found", 404);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve category with ID {Id}", query.CategoryId);
            return Result<Category>.Fail($"Failed to retrieve category: {ex.Message}", 500);
        }
    }
}