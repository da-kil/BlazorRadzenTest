using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.CategoryQueries;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/categories")]
[Authorize(Policy = "Employee")]
public class CategoriesController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<CategoriesController> logger;

    public CategoriesController(
        IQueryDispatcher queryDispatcher,
        ILogger<CategoriesController> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCategories([FromQuery] bool includeInactive = false)
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new CategoryListQuery { IncludeInactive = includeInactive });
            return CreateResponse(result, categories =>
            {
                return categories.Select(category => new CategoryDto
                {
                    Id = category.Id,
                    NameEn = category.NameEnglish,
                    NameDe = category.NameGerman,
                    DescriptionEn = category.DescriptionEnglish,
                    DescriptionDe = category.DescriptionGerman,
                    CreatedDate = category.CreatedDate,
                    IsActive = category.IsActive,
                    SortOrder = category.SortOrder
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving categories");
            return CreateResponse(Result<IEnumerable<CategoryDto>>.Fail("An error occurred while retrieving categories", 500));
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new CategoryQuery(id));
            if (result == null)
                return CreateResponse(Result<CategoryDto>.Fail($"Category with ID {id} not found", 404));

            return CreateResponse(result, category => new CategoryDto
            {
                Id = category.Id,
                NameEn = category.NameEnglish,
                NameDe = category.NameGerman,
                DescriptionEn = category.DescriptionEnglish,
                DescriptionDe = category.DescriptionGerman,
                CreatedDate = category.CreatedDate,
                IsActive = category.IsActive,
                SortOrder = category.SortOrder
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return CreateResponse(Result<CategoryDto>.Fail("An error occurred while retrieving the category", 500));
        }
    }
}