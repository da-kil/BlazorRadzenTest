using System.Collections.Concurrent;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Client-side implementation of UI translation service.
/// Provides cached translation lookup with local storage backup for Blazor WebAssembly.
/// Communicates with backend APIs for translation data.
/// Thread-safe singleton implementation for use across navigation.
/// </summary>
public class ClientUITranslationService : IUITranslationService
{
    private readonly HttpClient httpClient;
    private readonly ILocalStorageService localStorage;
    private readonly ConcurrentDictionary<string, UITranslation> _cache = new();
    private readonly TimeSpan cacheExpiry = TimeSpan.FromMinutes(30);
    private DateTime _lastCacheUpdate = DateTime.MinValue;
    private bool _cacheLoaded = false;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    private const string LocalStorageCacheKey = "uitranslations_cache";
    private const string LocalStorageCacheTimeKey = "uitranslations_cache_time";

    public ClientUITranslationService(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage)
    {
        this.httpClient = httpClientFactory.CreateClient("QueryClient");
        this.localStorage = localStorage;
    }

    public async Task<string> GetTextAsync(string key, Language language, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        await EnsureCacheLoadedAsync();

        if (_cache.TryGetValue(key, out var translation))
        {
            return translation.GetText(language);
        }

        // Try to load from server if not in cache
        try
        {
            var response = await httpClient.GetAsync($"/q/api/v1/translations/{Uri.EscapeDataString(key)}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var serverTranslation = JsonSerializer.Deserialize<UITranslation>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (serverTranslation != null)
                {
                    _cache[key] = serverTranslation;
                    await SaveCacheToLocalStorageAsync();
                    return serverTranslation.GetText(language);
                }
            }
        }
        catch
        {
            // Ignore server errors, fallback to key
        }

        // Check if we have a basic fallback for this key
        if (_cache.TryGetValue(key, out var fallbackTranslation))
        {
            return fallbackTranslation.GetText(language);
        }

        return key; // Final fallback if no translation found
    }

    public async Task<IList<UITranslation>> GetAllTranslationsAsync(CancellationToken cancellationToken = default)
    {
        await RefreshCacheFromServerAsync();
        return _cache.Values.ToList();
    }

    public async Task<IList<UITranslation>> GetTranslationsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        await EnsureCacheLoadedAsync();
        return _cache.Values.Where(t => t.Category == category).ToList();
    }

    public async Task<UITranslation> UpsertTranslationAsync(string key, string german, string english, string? category = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                Key = key,
                German = german,
                English = english,
                Category = category ?? "general"
            };

            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("/c/api/v1/translations", content, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var translation = JsonSerializer.Deserialize<UITranslation>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (translation != null)
                {
                    _cache[key] = translation;
                    await SaveCacheToLocalStorageAsync();
                    return translation;
                }
            }
        }
        catch
        {
            // Ignore errors, return basic translation
        }

        // Fallback: create local translation
        var fallbackTranslation = new UITranslation
        {
            Key = key,
            German = german,
            English = english,
            Category = category ?? "general"
        };
        _cache[key] = fallbackTranslation;
        return fallbackTranslation;
    }

    public async Task<bool> DeleteTranslationAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"/c/api/v1/translations/{Uri.EscapeDataString(key)}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _cache.TryRemove(key, out _);
                await SaveCacheToLocalStorageAsync();
                return true;
            }
        }
        catch
        {
            // Ignore errors
        }

        return false;
    }

    public async Task<bool> TranslationExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        await EnsureCacheLoadedAsync();
        return _cache.ContainsKey(key);
    }


    /// <summary>
    /// Gets all translation keys available in the system.
    /// Used for pre-loading optimization in components.
    /// </summary>
    public async Task<string[]> GetAllTranslationKeysAsync(CancellationToken cancellationToken = default)
    {
        await EnsureCacheLoadedAsync();
        return _cache.Keys.ToArray();
    }

    /// <summary>
    /// Gets multiple translations in a single batch for performance optimization.
    /// Returns a dictionary mapping translation keys to their text in the specified language.
    /// </summary>
    public async Task<Dictionary<string, string>> GetTranslationsAsync(string[] keys, Language language, CancellationToken cancellationToken = default)
    {
        if (keys == null || keys.Length == 0)
            return new Dictionary<string, string>();

        await EnsureCacheLoadedAsync();

        var result = new Dictionary<string, string>();
        foreach (var key in keys)
        {
            if (_cache.TryGetValue(key, out var translation))
            {
                result[key] = translation.GetText(language);
            }
            else
            {
                // Fallback to key if translation not found
                result[key] = key;
            }
        }

        return result;
    }

    /// <summary>
    /// Invalidates all frontend caches (memory cache and local storage) and forces a reload from server.
    /// This method should be called when the translation cache is cleared on the backend.
    /// Thread-safe implementation for singleton usage.
    /// </summary>
    public async Task InvalidateFrontendCacheAsync()
    {
        await _cacheLock.WaitAsync();
        try
        {
            // Clear memory cache
            _cache.Clear();

            // Clear LocalStorage
            await localStorage.RemoveItemAsync(LocalStorageCacheKey);
            await localStorage.RemoveItemAsync(LocalStorageCacheTimeKey);

            // Reset state to force reload from server
            _cacheLoaded = false;
            _lastCacheUpdate = DateTime.MinValue;
        }
        catch (Exception ex)
        {
            // Log error but don't throw - cache clearing should be resilient
            // The cache will be refreshed on next access anyway
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task EnsureCacheLoadedAsync()
    {
        // Fast path - check without lock
        if (_cacheLoaded && DateTime.UtcNow - _lastCacheUpdate < cacheExpiry)
        {
            return; // Cache is still valid
        }

        // Slow path - use semaphore for thread safety
        await _cacheLock.WaitAsync();
        try
        {
            // Double-check pattern - cache might have been loaded by another thread
            if (_cacheLoaded && DateTime.UtcNow - _lastCacheUpdate < cacheExpiry)
            {
                return; // Cache is now valid
            }

            // Try to load from local storage first
            if (!_cacheLoaded)
            {
                await LoadCacheFromLocalStorageAsync();
            }

            // If cache is expired or empty, refresh from server
            if (!_cacheLoaded || DateTime.UtcNow - _lastCacheUpdate >= cacheExpiry)
            {
                await RefreshCacheFromServerAsync();
            }
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task RefreshCacheFromServerAsync()
    {
        try
        {
            var response = await httpClient.GetAsync("/q/api/v1/translations");

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var translations = JsonSerializer.Deserialize<List<UITranslation>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (translations != null)
                {
                    _cache.Clear();
                    foreach (var translation in translations)
                    {
                        _cache[translation.Key] = translation;
                    }

                    _lastCacheUpdate = DateTime.UtcNow;
                    _cacheLoaded = true;

                    await SaveCacheToLocalStorageAsync();
                }
            }
        }
        catch
        {
            // Server unreachable and no cache - keys will be returned as-is
        }
    }

    private async Task SaveCacheToLocalStorageAsync()
    {
        try
        {
            var cacheData = _cache.Values.ToList();
            await localStorage.SetItemAsync(LocalStorageCacheKey, cacheData);
            await localStorage.SetItemAsync(LocalStorageCacheTimeKey, _lastCacheUpdate);
        }
        catch
        {
            // Ignore local storage errors
        }
    }

    private async Task LoadCacheFromLocalStorageAsync()
    {
        try
        {
            var cacheData = await localStorage.GetItemAsync<List<UITranslation>>(LocalStorageCacheKey);
            var cacheTime = await localStorage.GetItemAsync<DateTime>(LocalStorageCacheTimeKey);

            // Only load if data exists AND it's not expired
            if (cacheData != null && cacheData.Count > 0 &&
                cacheTime != DateTime.MinValue &&
                DateTime.UtcNow - cacheTime < cacheExpiry)
            {
                _cache.Clear();
                foreach (var translation in cacheData)
                {
                    _cache[translation.Key] = translation;
                }

                _lastCacheUpdate = cacheTime;
                _cacheLoaded = true;
            }
            else
            {
                // Clear localStorage if data is expired or invalid
                if (cacheData != null || cacheTime != DateTime.MinValue)
                {
                    await localStorage.RemoveItemAsync(LocalStorageCacheKey);
                    await localStorage.RemoveItemAsync(LocalStorageCacheTimeKey);
                }
            }
        }
        catch
        {
            // Ignore local storage errors
        }
    }

}

/// <summary>
/// Interface for client-side UI translation service.
/// Matches the server-side interface for consistency.
/// </summary>
public interface IUITranslationService
{
    Task<string> GetTextAsync(string key, Language language, CancellationToken cancellationToken = default);
    Task<IList<UITranslation>> GetAllTranslationsAsync(CancellationToken cancellationToken = default);
    Task<IList<UITranslation>> GetTranslationsByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<UITranslation> UpsertTranslationAsync(string key, string german, string english, string? category = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteTranslationAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> TranslationExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<string[]> GetAllTranslationKeysAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> GetTranslationsAsync(string[] keys, Language language, CancellationToken cancellationToken = default);
}