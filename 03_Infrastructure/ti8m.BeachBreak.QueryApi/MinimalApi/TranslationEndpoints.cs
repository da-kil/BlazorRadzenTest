using Microsoft.AspNetCore.Authorization;
using ti8m.BeachBreak.Application.Query.Mappers;
using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Core.Infrastructure.Services;

namespace ti8m.BeachBreak.QueryApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for translation queries.
/// </summary>
public static class TranslationEndpoints
{
    /// <summary>
    /// Maps translation query endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapTranslationEndpoints(this WebApplication app)
    {
        var translationGroup = app.MapGroup("/q/api/v{version:apiVersion}/translations")
            .WithTags("Translations")
            .RequireAuthorization();

        // Get translation by key
        translationGroup.MapGet("/{key}", async (
            string key,
            IUITranslationService translationService,
            ILanguageContext languageContext,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Results.BadRequest("Translation key cannot be empty");
            }

            try
            {
                var userLanguageCode = await languageContext.GetCurrentLanguageCodeAsync();
                var userLanguage = LanguageMapper.FromLanguageCode(userLanguageCode);
                logger.LogDebug("Getting translation for key: {Key} (user language: {Language})", key, userLanguage);

                // Get the text for current language
                var text = await translationService.GetTextAsync(key, userLanguage, cancellationToken);

                // Create a UITranslation object - we'll return the key as a fallback if not found
                var translation = new UITranslation
                {
                    Key = key,
                    English = text,  // For simplicity, we'll use the returned text
                    German = text,   // The service already handles language selection
                    Category = "dynamic"
                };

                return Results.Ok(translation);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving translation for key: {Key}", key);

                // Return the key as fallback
                var fallbackTranslation = new UITranslation
                {
                    Key = key,
                    English = key,
                    German = key,
                    Category = "fallback"
                };

                return Results.Ok(fallbackTranslation);
            }
        })
        .WithName("GetTranslation")
        .WithSummary("Get translation by key")
        .WithDescription("Get translation for a specific key")
        .Produces<UITranslation>(200)
        .Produces(400);

        // Get all translations
        translationGroup.MapGet("/", async (
            IUITranslationService translationService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                logger.LogInformation("Getting all translations - API called");
                var translations = await translationService.GetAllTranslationsAsync(cancellationToken);
                logger.LogInformation("Retrieved {Count} translations from service", translations?.Count ?? 0);

                // Log first few translations for debugging
                if (translations != null && translations.Any())
                {
                    foreach (var translation in translations.Take(3))
                    {
                        logger.LogInformation("Sample translation: Key='{Key}', English='{English}', German='{German}'",
                            translation.Key, translation.English, translation.German);
                    }
                }

                return Results.Ok(translations);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all translations");
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "Failed to retrieve translations",
                    statusCode: 500);
            }
        })
        .WithName("GetAllTranslations")
        .WithSummary("Get all translations")
        .WithDescription("Get all available translations")
        .Produces<IList<UITranslation>>(200)
        .Produces(500);

        // Get all translation keys
        translationGroup.MapGet("/keys", async (
            IUITranslationService translationService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                logger.LogDebug("Getting all translation keys");
                var translations = await translationService.GetAllTranslationsAsync(cancellationToken);
                var keys = translations.Select(t => t.Key).ToArray();
                return Results.Ok(keys);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving translation keys");
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "Failed to retrieve translation keys",
                    statusCode: 500);
            }
        })
        .WithName("GetAllTranslationKeys")
        .WithSummary("Get all translation keys")
        .WithDescription("Get all translation keys for pre-loading")
        .Produces<string[]>(200)
        .Produces(500);

        // Get translations by category
        translationGroup.MapGet("/category/{category}", async (
            string category,
            IUITranslationService translationService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                logger.LogDebug("Getting translations for category: {Category}", category);
                var translations = await translationService.GetTranslationsByCategoryAsync(category, cancellationToken);
                return Results.Ok(translations);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving translations for category: {Category}", category);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: $"Failed to retrieve translations for category: {category}",
                    statusCode: 500);
            }
        })
        .WithName("GetTranslationsByCategory")
        .WithSummary("Get translations by category")
        .WithDescription("Get translations for a specified category")
        .Produces<IList<UITranslation>>(200)
        .Produces(500);
    }
}