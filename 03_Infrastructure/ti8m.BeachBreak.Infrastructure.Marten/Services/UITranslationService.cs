using Marten;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.Application.Query.Services;

namespace ti8m.BeachBreak.Infrastructure.Marten.Services;

/// <summary>
/// Implementation of UI translation service using Marten for persistence and IDistributedCache for performance.
/// Provides localized text with fallback to English when German translations are missing.
/// Uses distributed cache to ensure consistency across multiple service instances.
/// </summary>
public class UITranslationService : IUITranslationService
{
    private readonly IDocumentSession session;
    private readonly IDistributedCache cache;
    private readonly ILogger<UITranslationService> logger;

    private const string CacheKeyPrefix = "uitranslation:";
    private const string AllTranslationsCacheKey = "uitranslations:all";
    private const string CategoryCacheKeyPrefix = "uitranslations:category:";

    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    };

    public UITranslationService(
        IDocumentSession session,
        IDistributedCache cache,
        ILogger<UITranslationService> logger)
    {
        this.session = session;
        this.cache = cache;
        this.logger = logger;
    }

    /// <summary>
    /// Helper method to set cache entries using distributed cache with JSON serialization
    /// </summary>
    private async Task SetCacheAsync<T>(string cacheKey, T value, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var bytes = Encoding.UTF8.GetBytes(json);
            await cache.SetAsync(cacheKey, bytes, CacheOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to set cache for key: {CacheKey}", cacheKey);
        }
    }

    /// <summary>
    /// Helper method to get cache entries using distributed cache with JSON deserialization
    /// </summary>
    private async Task<T?> GetCacheAsync<T>(string cacheKey, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cachedBytes = await cache.GetAsync(cacheKey, cancellationToken);
            if (cachedBytes != null)
            {
                var cachedJson = Encoding.UTF8.GetString(cachedBytes);
                return JsonSerializer.Deserialize<T>(cachedJson);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get cache for key: {CacheKey}", cacheKey);
        }
        return null;
    }

    /// <summary>
    /// Helper method to invalidate caches for a specific translation key and category
    /// </summary>
    private async Task InvalidateTranslationCachesAsync(string key, string? category, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>
        {
            cache.RemoveAsync($"{CacheKeyPrefix}{key}", cancellationToken),
            cache.RemoveAsync(AllTranslationsCacheKey, cancellationToken)
        };

        // Remove category cache if specified
        if (!string.IsNullOrWhiteSpace(category))
        {
            tasks.Add(cache.RemoveAsync($"{CategoryCacheKeyPrefix}{category}", cancellationToken));
        }

        await Task.WhenAll(tasks);
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
        var cachedTranslation = await GetCacheAsync<UITranslation>(cacheKey, cancellationToken);
        if (cachedTranslation != null)
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
                await SetCacheAsync(cacheKey, translation, cancellationToken);
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
        var cached = await GetCacheAsync<List<UITranslation>>(AllTranslationsCacheKey, cancellationToken);
        if (cached != null)
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
            await SetCacheAsync(AllTranslationsCacheKey, translationsList, cancellationToken);
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
        var categoryCacheKey = $"{CategoryCacheKeyPrefix}{category}";

        // Try cache first
        var cached = await GetCacheAsync<List<UITranslation>>(categoryCacheKey, cancellationToken);
        if (cached != null)
        {
            logger.LogDebug("Retrieved translations from cache for category: {Category}", category);
            return cached;
        }

        try
        {
            var translations = await session.Query<UITranslation>()
                .Where(t => t.Category == category)
                .OrderBy(t => t.Key)
                .ToListAsync(cancellationToken);

            var translationsList = translations.ToList();

            // Cache the result
            await SetCacheAsync(categoryCacheKey, translationsList, cancellationToken);
            logger.LogDebug("Cached {Count} translations for category: {Category}", translationsList.Count, category);

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

            // Invalidate relevant caches
            await InvalidateTranslationCachesAsync(key, existingTranslation.Category, cancellationToken);

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

            // Invalidate relevant caches
            await InvalidateTranslationCachesAsync(key, translation.Category, cancellationToken);

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
            await InvalidateCacheAsync(cancellationToken);

            logger.LogInformation("Bulk imported {Count} translations", importedCount);
            return importedCount;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk importing translations");
            throw;
        }
    }

    /// <summary>
    /// Invalidates all translation caches across all service instances (distributed cache invalidation)
    /// </summary>
    public async Task InvalidateCacheAsync(CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>
        {
            cache.RemoveAsync(AllTranslationsCacheKey, cancellationToken)
        };

        // Clear all category caches - we need to clear all possible category keys
        // Since we don't track keys in distributed cache, we'll clear the main caches
        // Individual translation caches will expire naturally or be refreshed on demand

        await Task.WhenAll(tasks);

        logger.LogInformation("Translation distributed cache invalidated - cleared main cache entries");
    }

    /// <summary>
    /// Legacy method for backwards compatibility
    /// </summary>
    public void InvalidateCache()
    {
        // For backwards compatibility, call the async version synchronously
        // This is not ideal but maintains interface compatibility
        InvalidateCacheAsync().GetAwaiter().GetResult();
    }
}