using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Core.Infrastructure.Services;
using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Application.Query.Mappers;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiVersion(1.0)]
[ApiController]
[Route("q/api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TranslationsController : ControllerBase
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
    public async Task<ActionResult<UITranslation>> GetTranslation(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest("Translation key cannot be empty");
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

            return Ok(translation);
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

            return Ok(fallbackTranslation);
        }
    }

    /// <summary>
    /// Get all available translations
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all translations</returns>
    [HttpGet]
    public async Task<ActionResult<IList<UITranslation>>> GetAllTranslations(CancellationToken cancellationToken = default)
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

            return Ok(translations);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all translations");
            return StatusCode(500, "Failed to retrieve translations");
        }
    }

    /// <summary>
    /// Get all translation keys (for pre-loading)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Array of translation keys</returns>
    [HttpGet("keys")]
    public async Task<ActionResult<string[]>> GetAllTranslationKeys(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Getting all translation keys");
            var translations = await translationService.GetAllTranslationsAsync(cancellationToken);
            var keys = translations.Select(t => t.Key).ToArray();
            return Ok(keys);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving translation keys");
            return StatusCode(500, "Failed to retrieve translation keys");
        }
    }

    /// <summary>
    /// Get translations by category
    /// </summary>
    /// <param name="category">Translation category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Translations for the specified category</returns>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IList<UITranslation>>> GetTranslationsByCategory(string category, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Getting translations for category: {Category}", category);
            var translations = await translationService.GetTranslationsByCategoryAsync(category, cancellationToken);
            return Ok(translations);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving translations for category: {Category}", category);
            return StatusCode(500, $"Failed to retrieve translations for category: {category}");
        }
    }

}