using ti8m.BeachBreak.Domain.CategoryAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.CategoryQueries;

public class CategoryListQuery : IQuery<Result<IEnumerable<Category>>>
{
    public bool IncludeInactive { get; init; } = false;
}