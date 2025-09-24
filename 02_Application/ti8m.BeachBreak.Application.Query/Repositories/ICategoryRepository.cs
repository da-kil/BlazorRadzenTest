using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Repositories;

public interface ICategoryRepository : IRepository
{
    Task<CategoryReadModel> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<CategoryReadModel>> GetAllCategoriesAsync(CancellationToken cancellationToken);
}
