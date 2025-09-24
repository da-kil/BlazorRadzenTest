using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.CategoryQueries;

public class CategoryQueryHandler :
    IQueryHandler<CategoryListQuery, Result<IEnumerable<Category>>>,
    IQueryHandler<CategoryQuery, Result<Category>>
{
    private readonly ICategoryRepository categoryRepository;
    private readonly ILogger<CategoryQueryHandler> logger;

    public CategoryQueryHandler(ICategoryRepository categoryRepository, ILogger<CategoryQueryHandler> logger)
    {
        this.categoryRepository = categoryRepository;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<Category>>> HandleAsync(CategoryListQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await categoryRepository.GetAllCategoriesAsync(cancellationToken);

            return Result<IEnumerable<Category>>.Success(categories.Select(o=>
            {
                return new Category
                {
                    Id = o.Id,
                    NameEnglish = o.Name.English,
                    NameGerman = o.Name.German,
                    DescriptionEnglish = o.Description.English,
                    DescriptionGerman = o.Description.German,
                    CreatedDate = o.CreatedDate,
                    SortOrder = o.SortOrder,
                    IsActive = o.IsActive
                };
            }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve categories");
            throw;
        }
    }

    public async Task<Result<Category>> HandleAsync(CategoryQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await categoryRepository.GetCategoryByIdAsync(query.CategoryId, cancellationToken);

            return Result<Category>.Success(new Category
            {
                Id = category.Id,
                NameEnglish = category.Name.English,
                NameGerman = category.Name.German,
                DescriptionEnglish = category.Description.English,
                DescriptionGerman = category.Description.German,
                CreatedDate = category.CreatedDate,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve category with ID {Id}", query.CategoryId);
            throw;
        }
    }
}