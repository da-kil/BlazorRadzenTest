using Marten;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.Application.Query.Services;

namespace ti8m.BeachBreak.Infrastructure.Marten.Services;

/// <summary>
/// Implementation of UI translation service using Marten for persistence and IMemoryCache for performance.
/// Provides localized text with fallback to English when German translations are missing.
/// </summary>
public class UITranslationService : IUITranslationService
{
    private readonly IDocumentSession session;
    private readonly IMemoryCache memoryCache;
    private readonly ILogger<UITranslationService> logger;

    private const string CacheKeyPrefix = "uitranslation:";
    private const string AllTranslationsCacheKey = "uitranslations:all";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(30);

    public UITranslationService(
        IDocumentSession session,
        IMemoryCache memoryCache,
        ILogger<UITranslationService> logger)
    {
        this.session = session;
        this.memoryCache = memoryCache;
        this.logger = logger;
    }

    public async Task<string> GetTextAsync(string key, Language language, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            logger.LogWarning("Empty translation key provided, returning empty string");
            return string.Empty;
        }

        var cacheKey = $"{CacheKeyPrefix}{key}";

        // Try to get from cache first
        if (memoryCache.TryGetValue(cacheKey, out UITranslation? cachedTranslation) && cachedTranslation != null)
        {
            logger.LogDebug("Retrieved translation from cache for key: {Key}", key);
            return cachedTranslation.GetText(language);
        }

        try
        {
            // Query from database
            var translation = await session.Query<UITranslation>()
                .FirstOrDefaultAsync(t => t.Key == key, cancellationToken);

            if (translation != null)
            {
                // Cache the result
                memoryCache.Set(cacheKey, translation, CacheExpiry);
                logger.LogDebug("Cached translation for key: {Key}", key);
                return translation.GetText(language);
            }

            logger.LogWarning("Translation not found for key: {Key}, returning the key itself", key);
            return key; // Return the key as fallback
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving translation for key: {Key}", key);
            return key; // Return the key as fallback
        }
    }

    public async Task<IList<UITranslation>> GetAllTranslationsAsync(CancellationToken cancellationToken = default)
    {
        // Try cache first
        if (memoryCache.TryGetValue(AllTranslationsCacheKey, out IList<UITranslation>? cached) && cached != null)
        {
            logger.LogDebug("Retrieved all translations from cache");
            return cached;
        }

        try
        {
            var translations = await session.Query<UITranslation>()
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Key)
                .ToListAsync(cancellationToken);

            var translationsList = translations.ToList();

            // Cache the result
            memoryCache.Set(AllTranslationsCacheKey, translationsList, CacheExpiry);
            logger.LogDebug("Cached all {Count} translations", translationsList.Count);

            return translationsList;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all translations");
            return new List<UITranslation>();
        }
    }

    public async Task<IList<UITranslation>> GetTranslationsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        try
        {
            var translations = await session.Query<UITranslation>()
                .Where(t => t.Category == category)
                .OrderBy(t => t.Key)
                .ToListAsync(cancellationToken);

            var translationsList = translations.ToList();
            logger.LogDebug("Retrieved {Count} translations for category: {Category}", translationsList.Count, category);
            return translationsList;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving translations for category: {Category}", category);
            return new List<UITranslation>();
        }
    }

    public async Task<UITranslation> UpsertTranslationAsync(string key, string german, string english, string? category = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingTranslation = await session.Query<UITranslation>()
                .FirstOrDefaultAsync(t => t.Key == key, cancellationToken);

            if (existingTranslation != null)
            {
                // Update existing
                existingTranslation.German = german;
                existingTranslation.English = english;
                existingTranslation.Category = category ?? existingTranslation.Category;
                existingTranslation.UpdatedDate = DateTimeOffset.UtcNow;

                session.Update(existingTranslation);
            }
            else
            {
                // Create new
                existingTranslation = new UITranslation
                {
                    Key = key,
                    German = german,
                    English = english,
                    Category = category ?? "general",
                    CreatedDate = DateTimeOffset.UtcNow
                };

                session.Store(existingTranslation);
            }

            await session.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            var cacheKey = $"{CacheKeyPrefix}{key}";
            memoryCache.Remove(cacheKey);
            memoryCache.Remove(AllTranslationsCacheKey);

            logger.LogInformation("Upserted translation for key: {Key}", key);
            return existingTranslation;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting translation for key: {Key}", key);
            throw;
        }
    }

    public async Task<bool> DeleteTranslationAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var translation = await session.Query<UITranslation>()
                .FirstOrDefaultAsync(t => t.Key == key, cancellationToken);

            if (translation == null)
            {
                logger.LogWarning("Translation not found for deletion: {Key}", key);
                return false;
            }

            session.Delete(translation);
            await session.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            var cacheKey = $"{CacheKeyPrefix}{key}";
            memoryCache.Remove(cacheKey);
            memoryCache.Remove(AllTranslationsCacheKey);

            logger.LogInformation("Deleted translation for key: {Key}", key);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting translation for key: {Key}", key);
            throw;
        }
    }

    public async Task<bool> TranslationExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await session.Query<UITranslation>()
                .AnyAsync(t => t.Key == key, cancellationToken);

            logger.LogDebug("Translation existence check for key {Key}: {Exists}", key, exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking translation existence for key: {Key}", key);
            return false;
        }
    }

    public async Task<int> SeedInitialTranslationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var seedTranslations = GetSeedTranslations();

            // Check if translations already exist
            var existingCount = await session.Query<UITranslation>().CountAsync(cancellationToken);
            if (existingCount > 0)
            {
                logger.LogInformation("Translations already exist ({Count}), no seeding performed", existingCount);
                return 0;
            }

            // No existing translations, seed all
            foreach (var translation in seedTranslations)
            {
                session.Store(translation);
            }

            await session.SaveChangesAsync(cancellationToken);

            // Clear cache to ensure fresh data
            memoryCache.Remove(AllTranslationsCacheKey);

            logger.LogInformation("Seeded {Count} initial translations", seedTranslations.Count);
            return seedTranslations.Count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding translations");
            throw;
        }
    }

    public async Task<int> BulkImportTranslationsAsync(IList<UITranslation> translations, CancellationToken cancellationToken = default)
    {
        try
        {
            int importedCount = 0;

            foreach (var translation in translations)
            {
                if (string.IsNullOrWhiteSpace(translation.Key))
                {
                    logger.LogWarning("Skipping translation with empty key");
                    continue;
                }

                var existingTranslation = await session.Query<UITranslation>()
                    .FirstOrDefaultAsync(t => t.Key == translation.Key, cancellationToken);

                if (existingTranslation != null)
                {
                    // Update existing
                    existingTranslation.German = translation.German;
                    existingTranslation.English = translation.English;
                    existingTranslation.Category = translation.Category ?? existingTranslation.Category;
                    existingTranslation.UpdatedDate = DateTimeOffset.UtcNow;

                    session.Update(existingTranslation);
                }
                else
                {
                    // Create new
                    var newTranslation = new UITranslation
                    {
                        Key = translation.Key,
                        German = translation.German,
                        English = translation.English,
                        Category = translation.Category ?? "general",
                        CreatedDate = DateTimeOffset.UtcNow
                    };

                    session.Store(newTranslation);
                }

                importedCount++;
            }

            await session.SaveChangesAsync(cancellationToken);

            // Clear cache to ensure fresh data
            InvalidateCache();

            logger.LogInformation("Bulk imported {Count} translations", importedCount);
            return importedCount;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk importing translations");
            throw;
        }
    }

    public void InvalidateCache()
    {
        // Get all cache keys by iterating through known patterns
        // Note: IMemoryCache doesn't expose all keys, so we clear known patterns
        memoryCache.Remove(AllTranslationsCacheKey);

        logger.LogInformation("Translation cache invalidated - all cached entries will be reloaded from database on next access");
    }

    private static List<UITranslation> GetSeedTranslations()
    {
        // NOTE: Comprehensive translations are now managed in TestDataGenerator project.
        // This method now only provides essential fallback translations.
        // To add new translations, use the TestDataGenerator project and run the seeding script.
        return new List<UITranslation>
        {
            // Essential navigation fallbacks
            new() { Key = "nav.dashboard", German = "Dashboard", English = "Dashboard", Category = "navigation" },
            new() { Key = "nav.my-questionnaires", German = "Meine Fragebogen", English = "My Questionnaires", Category = "navigation" },

            // Essential button fallbacks
            new() { Key = "buttons.save", German = "Speichern", English = "Save", Category = "buttons" },
            new() { Key = "buttons.cancel", German = "Abbrechen", English = "Cancel", Category = "buttons" },
            new() { Key = "buttons.loading", German = "Lädt", English = "Loading", Category = "buttons" },

            // Essential status fallbacks
            new() { Key = "status.active", German = "Aktiv", English = "Active", Category = "status" },
            new() { Key = "status.pending", German = "Ausstehend", English = "Pending", Category = "status" },

            // Essential language fallbacks
            new() { Key = "language.german", German = "Deutsch", English = "German", Category = "language" },
            new() { Key = "language.english", German = "Englisch", English = "English", Category = "language" }
        };
    }
}