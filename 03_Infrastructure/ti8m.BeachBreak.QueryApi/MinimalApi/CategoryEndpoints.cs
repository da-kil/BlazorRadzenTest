using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.CategoryQueries;
using ti8m.BeachBreak.QueryApi.Dto;
using ti8m.BeachBreak.Core.Infrastructure;

namespace ti8m.BeachBreak.QueryApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for category queries.
/// </summary>
public static class CategoryEndpoints
{
    /// <summary>
    /// Maps category query endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var categoryGroup = app.MapGroup("/q/api/v{version:apiVersion}/categories")
            .WithTags("Categories")
            .RequireAuthorization("Employee");

        // Get all categories
        categoryGroup.MapGet("/", async (
            IQueryDispatcher queryDispatcher,
            [FromServices] ILogger logger,
            bool includeInactive = false,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var result = await queryDispatcher.QueryAsync(new CategoryListQuery { IncludeInactive = includeInactive }, cancellationToken);

                if (result.Succeeded)
                {
                    var categories = result.Payload.Select(category => new CategoryDto
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

                    return Results.Ok(categories);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogCategoriesRetrievalError(ex);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving categories",
                    statusCode: 500);
            }
        })
        .WithName("GetAllCategories")
        .WithSummary("Get all categories")
        .WithDescription("Retrieves all categories with optional inactive categories")
        .Produces<IEnumerable<CategoryDto>>(200)
        .Produces(500);

        // Get category by ID
        categoryGroup.MapGet("/{id:guid}", async (
            Guid id,
            IQueryDispatcher queryDispatcher,
            [FromServices] ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var result = await queryDispatcher.QueryAsync(new CategoryQuery(id), cancellationToken);
                if (result == null)
                {
                    return Results.NotFound($"Category with ID {id} not found");
                }

                if (result.Succeeded)
                {
                    var category = new CategoryDto
                    {
                        Id = result.Payload.Id,
                        NameEn = result.Payload.NameEnglish,
                        NameDe = result.Payload.NameGerman,
                        DescriptionEn = result.Payload.DescriptionEnglish,
                        DescriptionDe = result.Payload.DescriptionGerman,
                        CreatedDate = result.Payload.CreatedDate,
                        IsActive = result.Payload.IsActive,
                        SortOrder = result.Payload.SortOrder
                    };

                    return Results.Ok(category);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogCategoryRetrievalError(id, ex);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving the category",
                    statusCode: 500);
            }
        })
        .WithName("GetCategory")
        .WithSummary("Get category by ID")
        .WithDescription("Retrieves a specific category by its ID")
        .Produces<CategoryDto>(200)
        .Produces(404)
        .Produces(500);
    }
}