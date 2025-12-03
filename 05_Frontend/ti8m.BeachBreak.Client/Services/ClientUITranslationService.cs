using System.Text.Json;
using Blazored.LocalStorage;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Client-side implementation of UI translation service.
/// Provides cached translation lookup with local storage backup for Blazor WebAssembly.
/// Communicates with backend APIs for translation data.
/// </summary>
public class ClientUITranslationService : IUITranslationService
{
    private readonly HttpClient httpClient;
    private readonly ILocalStorageService localStorage;
    private readonly Dictionary<string, UITranslation> _cache = new();
    private readonly TimeSpan cacheExpiry = TimeSpan.FromMinutes(30);
    private DateTime _lastCacheUpdate = DateTime.MinValue;
    private bool _cacheLoaded = false;

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
                _cache.Remove(key);
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

    private async Task EnsureCacheLoadedAsync()
    {
        if (_cacheLoaded && DateTime.UtcNow - _lastCacheUpdate < cacheExpiry)
        {
            return; // Cache is still valid
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
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception ex)
        {
            // If server fails and we have no cache, load basic fallbacks
            if (!_cacheLoaded)
            {
                LoadBasicFallbackTranslations();
            }
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

            if (cacheData != null && cacheData.Count > 0)
            {
                _cache.Clear();
                foreach (var translation in cacheData)
                {
                    _cache[translation.Key] = translation;
                }

                _lastCacheUpdate = cacheTime;
                _cacheLoaded = true;
            }
        }
        catch
        {
            // Ignore local storage errors
        }
    }

    private void LoadBasicFallbackTranslations()
    {
        // Basic fallback translations for when both server and local storage fail
        var fallbacks = new[]
        {
            // Navigation - CRITICAL for NavMenu to work
            new UITranslation { Key = "nav.menu-toggle", German = "Navigationsmenü", English = "Navigation menu", Category = "navigation" },
            new UITranslation { Key = "nav.dashboard", German = "Dashboard", English = "Dashboard", Category = "navigation" },
            new UITranslation { Key = "nav.my-work", German = "Meine Arbeit", English = "My Work", Category = "navigation" },
            new UITranslation { Key = "nav.my-questionnaires", German = "Meine Fragebogen", English = "My Questionnaires", Category = "navigation" },
            new UITranslation { Key = "nav.team-overview", German = "Team Übersicht", English = "Team Overview", Category = "navigation" },
            new UITranslation { Key = "nav.organization", German = "Organisation", English = "Organization", Category = "navigation" },
            new UITranslation { Key = "nav.management", German = "Verwaltung", English = "Management", Category = "navigation" },
            new UITranslation { Key = "nav.create-questionnaire", German = "Fragebogen erstellen", English = "Create Questionnaire", Category = "navigation" },
            new UITranslation { Key = "nav.manage-questionnaires", German = "Fragebogen verwalten", English = "Manage Questionnaires", Category = "navigation" },
            new UITranslation { Key = "nav.assignments", German = "Zuweisungen", English = "Assignments", Category = "navigation" },
            new UITranslation { Key = "nav.administration", German = "Administration", English = "Administration", Category = "navigation" },
            new UITranslation { Key = "nav.categories", German = "Kategorien", English = "Categories", Category = "navigation" },
            new UITranslation { Key = "nav.role-management", German = "Rollenverwaltung", English = "Role Management", Category = "navigation" },
            new UITranslation { Key = "nav.projection-replay", German = "Projektions-Wiederholung", English = "Projection Replay", Category = "navigation" },
            // Common buttons
            new UITranslation { Key = "buttons.save", German = "Speichern", English = "Save", Category = "buttons" },
            new UITranslation { Key = "buttons.cancel", German = "Abbrechen", English = "Cancel", Category = "buttons" },
            new UITranslation { Key = "buttons.delete", German = "Löschen", English = "Delete", Category = "buttons" },
            new UITranslation { Key = "buttons.edit", German = "Bearbeiten", English = "Edit", Category = "buttons" },
            new UITranslation { Key = "validation.required", German = "Erforderlich", English = "Required", Category = "validation" }
        };

        foreach (var fallback in fallbacks)
        {
            _cache[fallback.Key] = fallback;
        }

        _cacheLoaded = true;
        _lastCacheUpdate = DateTime.UtcNow;
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