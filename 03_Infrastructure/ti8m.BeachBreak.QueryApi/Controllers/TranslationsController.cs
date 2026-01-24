using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Core.Infrastructure.Services;
using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Application.Query.Mappers;
using ti8m.BeachBreak.Application.Query.Queries;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiVersion(1.0)]
[ApiController]
[Route("q/api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TranslationsController : BaseController
{
    private readonly IUITranslationService translationService;
    private readonly ILanguageContext languageContext;
    private readonly ILogger<TranslationsController> logger;

    public TranslationsController(
        IUITranslationService translationService,
        ILanguageContext languageContext,
        ILogger<TranslationsController> logger)
    {
        this.translationService = translationService;
        this.languageContext = languageContext;
        this.logger = logger;
    }

    /// <summary>
    /// Get translation for a specific key
    /// </summary>
    /// <param name="key">Translation key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Translation object with English and German text</returns>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetTranslation(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return CreateResponse(Result<UITranslation>.Fail("Translation key cannot be empty", 400));
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

            return CreateResponse(Result<UITranslation>.Success(translation));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving translation for key: {Key}", key);

            // Return the key as fallback - legitimate local error handling for graceful degradation
            var fallbackTranslation = new UITranslation
            {
                Key = key,
                English = key,
                German = key,
                Category = "fallback"
            };

            return CreateResponse(Result<UITranslation>.Success(fallbackTranslation));
        }
    }

    /// <summary>
    /// Get all available translations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all translations</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllTranslations(CancellationToken cancellationToken = default)
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

        return CreateResponse(Result<IList<UITranslation>>.Success(translations));
    }

    /// <summary>
    /// Get all translation keys (for pre-loading)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of translation keys</returns>
    [HttpGet("keys")]
    public async Task<IActionResult> GetAllTranslationKeys(CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting all translation keys");
        var translations = await translationService.GetAllTranslationsAsync(cancellationToken);
        var keys = translations.Select(t => t.Key).ToArray();
        return CreateResponse(Result<string[]>.Success(keys));
    }

    /// <summary>
    /// Get translations by category
    /// </summary>
    /// <param name="category">Translation category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Translations for the specified category</returns>
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetTranslationsByCategory(string category, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting translations for category: {Category}", category);
        var translations = await translationService.GetTranslationsByCategoryAsync(category, cancellationToken);
        return CreateResponse(Result<IList<UITranslation>>.Success(translations));
    }

}