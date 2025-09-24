using ti8m.BeachBreak.Domain.CategoryAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.CategoryQueries;

public class CategoryQuery : IQuery<Result<Category>>
{
    public Guid CategoryId { get; init; }

    public CategoryQuery(Guid categoryId)
    {
        CategoryId = categoryId;
    }
}