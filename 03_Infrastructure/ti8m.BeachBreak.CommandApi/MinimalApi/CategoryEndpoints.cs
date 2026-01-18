using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;
using ti8m.BeachBreak.CommandApi.Dto;

namespace ti8m.BeachBreak.CommandApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for category management.
/// </summary>
public static class CategoryEndpoints
{
    /// <summary>
    /// Maps category management endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var categoryGroup = app.MapGroup("/c/api/v{version:apiVersion}/categories")
            .WithTags("Categories")
            .RequireAuthorization("HROrApp"); // Allows HR users OR service principals with DataSeeder app role

        // Create category
        categoryGroup.MapPost("/", async (
            CategoryDto categoryDto,
            ICommandDispatcher commandDispatcher,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(categoryDto.NameEn))
                {
                    return Results.BadRequest("Category English name is required");
                }

                if (string.IsNullOrWhiteSpace(categoryDto.NameDe))
                {
                    return Results.BadRequest("Category German name is required");
                }

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

                Result result = await commandDispatcher.SendAsync(new CreateCategoryCommand(category), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Category created successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Category creation failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating category");
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while creating the category",
                    statusCode: 500);
            }
        })
        .WithName("CreateCategory")
        .WithSummary("Create a new category")
        .WithDescription("Creates a new questionnaire category")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Update category
        categoryGroup.MapPut("/{id:guid}", async (
            Guid id,
            CategoryDto categoryDto,
            ICommandDispatcher commandDispatcher,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(categoryDto.NameEn))
                {
                    return Results.BadRequest("Category English name is required");
                }

                if (string.IsNullOrWhiteSpace(categoryDto.NameDe))
                {
                    return Results.BadRequest("Category German name is required");
                }

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

                Result result = await commandDispatcher.SendAsync(new UpdateCategoryCommand(category), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Category updated successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Category update failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating category {CategoryId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while updating the category",
                    statusCode: 500);
            }
        })
        .WithName("UpdateCategory")
        .WithSummary("Update an existing category")
        .WithDescription("Updates an existing questionnaire category")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Deactivate category
        categoryGroup.MapDelete("/{id:guid}", async (
            Guid id,
            ICommandDispatcher commandDispatcher,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                Result result = await commandDispatcher.SendAsync(new DeactivateCategoryCommand(id), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Category deactivated successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Category deactivation failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deactivating category {CategoryId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while deactivating the category",
                    statusCode: 500);
            }
        })
        .WithName("DeactivateCategory")
        .WithSummary("Deactivate a category")
        .WithDescription("Deactivates a questionnaire category (soft delete)")
        .Produces(200)
        .Produces(400)
        .Produces(500);
    }
}