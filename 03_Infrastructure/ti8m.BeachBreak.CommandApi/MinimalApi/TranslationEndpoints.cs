using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Core.Infrastructure;

namespace ti8m.BeachBreak.CommandApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for translation management.
/// </summary>
public static class TranslationEndpoints
{
    /// <summary>
    /// Maps translation management endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapTranslationEndpoints(this WebApplication app)
    {
        var translationGroup = app.MapGroup("/c/api/v{version:apiVersion}/translations")
            .WithTags("Translations")
            .RequireAuthorization("AdminOrApp");

        // Upsert translation
        translationGroup.MapPost("/", async (
            UpsertTranslationRequest request,
            IUITranslationService translationService,
            [FromServices] ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Key))
                {
                    return Results.Problem(
                        title: "Invalid Request",
                        detail: "Translation key cannot be empty",
                        statusCode: 400);
                }

                logger.LogUpsertTranslationRequest(request.Key);

                var translation = await translationService.UpsertTranslationAsync(
                    request.Key,
                    request.German,
                    request.English,
                    request.Category,
                    cancellationToken);

                logger.LogUpsertTranslationSuccess(request.Key);

                return Results.Ok($"Translation '{request.Key}' upserted successfully");
            }
            catch (Exception ex)
            {
                logger.LogUpsertTranslationError(ex, request.Key);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "Failed to upsert translation",
                    statusCode: 500);
            }
        })
        .WithName("UpsertTranslation")
        .WithSummary("Create or update a translation")
        .WithDescription("Creates a new translation or updates an existing one")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Delete translation
        translationGroup.MapDelete("/{key}", async (
            string key,
            IUITranslationService translationService,
            [FromServices] ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    return Results.Problem(
                        title: "Invalid Request",
                        detail: "Translation key cannot be empty",
                        statusCode: 400);
                }

                logger.LogDeleteTranslationRequest(key);

                var success = await translationService.DeleteTranslationAsync(key, cancellationToken);

                if (success)
                {
                    logger.LogDeleteTranslationSuccess(key);
                    return Results.Ok("Translation deleted successfully");
                }
                else
                {
                    logger.LogDeleteTranslationNotFound(key);
                    return Results.NotFound("Translation not found");
                }
            }
            catch (Exception ex)
            {
                logger.LogDeleteTranslationError(ex, key);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "Failed to delete translation",
                    statusCode: 500);
            }
        })
        .WithName("DeleteTranslation")
        .WithSummary("Delete a translation")
        .WithDescription("Deletes a translation by key")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .Produces(500);

        // Bulk import translations
        translationGroup.MapPost("/bulk-import", async (
            List<UpsertTranslationRequest> translations,
            IUITranslationService translationService,
            [FromServices] ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                if (translations == null || translations.Count == 0)
                {
                    return Results.Problem(
                        title: "Invalid Request",
                        detail: "No translations provided",
                        statusCode: 400);
                }

                logger.LogBulkImportTranslationsRequest(translations.Count);

                // Convert requests to UITranslation objects
                var uiTranslations = translations.Select(t => new Application.Query.Models.UITranslation
                {
                    Key = t.Key,
                    German = t.German,
                    English = t.English,
                    Category = t.Category ?? "general",
                    CreatedDate = DateTimeOffset.UtcNow
                }).ToList();

                var importCount = await translationService.BulkImportTranslationsAsync(uiTranslations, cancellationToken);

                logger.LogBulkImportTranslationsSuccess(importCount);

                return Results.Ok(importCount);
            }
            catch (Exception ex)
            {
                logger.LogBulkImportTranslationsError(ex);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "Failed to bulk import translations",
                    statusCode: 500);
            }
        })
        .WithName("BulkImportTranslations")
        .WithSummary("Bulk import translations")
        .WithDescription("Imports multiple translations from a JSON file - Admin only")
        .Produces<int>(200)
        .Produces(400)
        .Produces(500);

        // Invalidate cache
        translationGroup.MapPost("/invalidate-cache", (
            IUITranslationService translationService,
            [FromServices] ILogger logger) =>
        {
            try
            {
                logger.LogInvalidateCacheRequest();
                translationService.InvalidateCache();

                logger.LogInvalidateCacheSuccess();

                return Results.Ok("Translation cache invalidated successfully");
            }
            catch (Exception ex)
            {
                logger.LogInvalidateCacheError(ex);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "Failed to invalidate translation cache",
                    statusCode: 500);
            }
        })
        .WithName("InvalidateTranslationCache")
        .WithSummary("Invalidate translation cache")
        .WithDescription("Forces reload from database on next access - useful for containerized environments")
        .Produces(200)
        .Produces(500);
    }
}

/// <summary>
/// DTO for translation upsert requests
/// </summary>
public class UpsertTranslationRequest
{
    public string Key { get; set; } = string.Empty;
    public string German { get; set; } = string.Empty;
    public string English { get; set; } = string.Empty;
    public string? Category { get; set; }
}