using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;
using ti8m.BeachBreak.CommandApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/categories")]
[Authorize(Policy = "HROrApp")] // Allows HR users OR service principals with DataSeeder app role
public class CategoriesController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly ILogger<CategoriesController> logger;

    public CategoriesController(
        ICommandDispatcher commandDispatcher,
        ILogger<CategoriesController> logger)
    {
        this.commandDispatcher = commandDispatcher;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CategoryDto categoryDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return CreateResponse(Result.Fail("Invalid model state", 400));

            if (string.IsNullOrWhiteSpace(categoryDto.NameEn))
                return CreateResponse(Result.Fail("Category English name is required", 400));

            if (string.IsNullOrWhiteSpace(categoryDto.NameDe))
                return CreateResponse(Result.Fail("Category German name is required", 400));

            var category = new CommandCategory
            {
                Id = categoryDto.Id,
                NameEn = categoryDto.NameEn.Trim(),
                NameDe = categoryDto.NameDe.Trim(),
                DescriptionEn = categoryDto.DescriptionEn?.Trim() ?? string.Empty,
                DescriptionDe = categoryDto.DescriptionDe?.Trim() ?? string.Empty,
                IsActive = categoryDto.IsActive,
                SortOrder = categoryDto.SortOrder
            };

            Result result = await commandDispatcher.SendAsync(new CreateCategoryCommand(category));

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating category");
            return CreateResponse(Result.Fail("An error occurred while creating the category", 500));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, CategoryDto categoryDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return CreateResponse(Result.Fail("Invalid model state", 400));

            if (string.IsNullOrWhiteSpace(categoryDto.NameEn))
                return CreateResponse(Result.Fail("Category English name is required", 400));

            if (string.IsNullOrWhiteSpace(categoryDto.NameDe))
                return CreateResponse(Result.Fail("Category German name is required", 400));

            var category = new CommandCategory
            {
                Id = id,
                NameEn = categoryDto.NameEn.Trim(),
                NameDe = categoryDto.NameDe.Trim(),
                DescriptionEn = categoryDto.DescriptionEn?.Trim() ?? string.Empty,
                DescriptionDe = categoryDto.DescriptionDe?.Trim() ?? string.Empty,
                IsActive = categoryDto.IsActive,
                SortOrder = categoryDto.SortOrder
            };

            Result result = await commandDispatcher.SendAsync(new UpdateCategoryCommand(category));

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating category {CategoryId}", id);
            return CreateResponse(Result.Fail("An error occurred while updating the category", 500));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeactivateCategory(Guid id)
    {
        try
        {
            Result result = await commandDispatcher.SendAsync(new DeactivateCategoryCommand(id));

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating category {CategoryId}", id);
            return CreateResponse(Result.Fail("An error occurred while deactivating the category", 500));
        }
    }
}