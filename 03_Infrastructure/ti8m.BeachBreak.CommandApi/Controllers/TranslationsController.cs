using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Services;
using CommandResult = ti8m.BeachBreak.Application.Command.Commands.Result;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiVersion(1.0)]
[ApiController]
[Route("c/api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminOrApp")]  // Only admins can seed translations
public class TranslationsController : BaseController
{
    private readonly IUITranslationService translationService;
    private readonly ILogger<TranslationsController> logger;

    public TranslationsController(
        IUITranslationService translationService,
        ILogger<TranslationsController> logger)
    {
        this.translationService = translationService;
        this.logger = logger;
    }

    /// <summary>
    /// Create or update a translation
    /// </summary>
    /// <param name="request">Translation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created/updated translation</returns>
    [HttpPost]
    public async Task<IActionResult> UpsertTranslation([FromBody] UpsertTranslationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Key))
            {
                return CreateResponse(CommandResult.Fail("Translation key cannot be empty", 400));
            }

            logger.LogInformation("Upserting translation for key: {Key}", request.Key);
            var translation = await translationService.UpsertTranslationAsync(
                request.Key,
                request.German,
                request.English,
                request.Category,
                cancellationToken);

            logger.LogInformation("Successfully upserted translation for key: {Key}", request.Key);
            return CreateResponse(CommandResult.Success($"Translation '{request.Key}' upserted successfully"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting translation for key: {Key}", request.Key);
            return CreateResponse(CommandResult.Fail("Failed to upsert translation", 500));
        }
    }

    /// <summary>
    /// Delete a translation
    /// </summary>
    /// <param name="key">Translation key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{key}")]
    public async Task<IActionResult> DeleteTranslation(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return CreateResponse(CommandResult.Fail("Translation key cannot be empty", 400));
            }

            logger.LogInformation("Deleting translation for key: {Key}", key);
            var success = await translationService.DeleteTranslationAsync(key, cancellationToken);

            if (success)
            {
                logger.LogInformation("Successfully deleted translation for key: {Key}", key);
                return CreateResponse(CommandResult.Success());
            }
            else
            {
                logger.LogWarning("Translation not found for deletion: {Key}", key);
                return CreateResponse(CommandResult.Fail("Translation not found", 404));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting translation for key: {Key}", key);
            return CreateResponse(CommandResult.Fail("Failed to delete translation", 500));
        }
    }

    /// <summary>
    /// Seed initial translations - Admin only command operation
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of translations seeded</returns>
    [HttpPost("seed")]
    public async Task<IActionResult> SeedTranslations(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Seeding translations requested by admin");
            var seedCount = await translationService.SeedInitialTranslationsAsync(cancellationToken);

            if (seedCount > 0)
            {
                logger.LogInformation("Successfully seeded {Count} translations", seedCount);
                return CreateResponse(CommandResult.Success(seedCount));
            }
            else
            {
                logger.LogInformation("Translations already exist, no seeding performed");
                return CreateResponse(CommandResult.Success(0));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding translations");
            return CreateResponse(CommandResult.Fail("Failed to seed translations", 500));
        }
    }

    /// <summary>
    /// Bulk import translations from JSON file - Admin only command operation
    /// </summary>
    /// <param name="translations">List of translations to import</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of translations imported</returns>
    [HttpPost("bulk-import")]
    public async Task<IActionResult> BulkImportTranslations([FromBody] List<UpsertTranslationRequest> translations, CancellationToken cancellationToken = default)
    {
        try
        {
            if (translations == null || translations.Count == 0)
            {
                return CreateResponse(CommandResult.Fail("No translations provided", 400));
            }

            logger.LogInformation("Bulk importing {Count} translations requested by admin", translations.Count);

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

            logger.LogInformation("Successfully bulk imported {Count} translations", importCount);
            return CreateResponse(CommandResult.Success(importCount));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk importing translations");
            return CreateResponse(CommandResult.Fail("Failed to bulk import translations", 500));
        }
    }

    /// <summary>
    /// Invalidate translation cache - Forces reload from database on next access
    /// Useful for containerized environments where cache needs to be cleared
    /// </summary>
    /// <returns>Success status</returns>
    [HttpPost("invalidate-cache")]
    public IActionResult InvalidateCache()
    {
        try
        {
            logger.LogInformation("Translation cache invalidation requested by admin");
            translationService.InvalidateCache();

            logger.LogInformation("Successfully invalidated translation cache");
            return CreateResponse(CommandResult.Success("Translation cache invalidated successfully"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating translation cache");
            return CreateResponse(CommandResult.Fail("Failed to invalidate translation cache", 500));
        }
    }

    public class UpsertTranslationRequest
    {
        public string Key { get; set; } = string.Empty;
        public string German { get; set; } = string.Empty;
        public string English { get; set; } = string.Empty;
        public string? Category { get; set; }
    }
}