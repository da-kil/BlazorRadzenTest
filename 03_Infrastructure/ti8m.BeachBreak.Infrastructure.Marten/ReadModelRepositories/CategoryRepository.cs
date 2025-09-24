using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

internal class CategoryRepository(IDocumentStore store) : ICategoryRepository
{
    public async Task<IEnumerable<CategoryReadModel>> GetAllCategoriesAsync(CancellationToken cancellationToken)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<CategoryReadModel>()
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryReadModel> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<CategoryReadModel>().SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}
