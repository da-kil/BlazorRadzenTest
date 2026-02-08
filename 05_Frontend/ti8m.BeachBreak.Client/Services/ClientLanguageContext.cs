using System.Text.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Client-side implementation of language context service.
/// Provides language preference management for Blazor WebAssembly application.
/// Communicates with backend APIs for language preference storage.
/// </summary>
public class ClientLanguageContext : ILanguageContext
{
    private readonly HttpClient httpClient;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IAuthService authService;
    private readonly TimeSpan cacheExpiry = TimeSpan.FromMinutes(10);
    private volatile TaskCompletionSource<Language>? refreshTaskSource;

    private class CacheData
    {
        public Language? CachedLanguage { get; set; }
        public DateTime LastCacheUpdate { get; set; }

        public CacheData(Language? language, DateTime lastUpdate)
        {
            CachedLanguage = language;
            LastCacheUpdate = lastUpdate;
        }
    }

    private volatile CacheData cache = new CacheData(null, DateTime.MinValue);

    public ClientLanguageContext(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        this.httpClient = httpClientFactory.CreateClient("QueryClient");
        this.httpClientFactory = httpClientFactory;
        this.authService = authService;
    }


    public async Task<Language> GetCurrentLanguageAsync()
    {
        var currentCache = cache; // Volatile read

        // Fast path - check cache validity
        if (currentCache.CachedLanguage.HasValue && DateTime.UtcNow - currentCache.LastCacheUpdate < cacheExpiry)
        {
            return currentCache.CachedLanguage.Value;
        }

        // Try to start a refresh operation atomically
        var tcs = new TaskCompletionSource<Language>();
        var existingTcs = Interlocked.CompareExchange(ref refreshTaskSource, tcs, null);

        if (existingTcs == null)
        {
            // We won the race, do the refresh
            try
            {
                // Double-check - cache might have been updated by another thread
                currentCache = cache;
                if (currentCache.CachedLanguage.HasValue && DateTime.UtcNow - currentCache.LastCacheUpdate < cacheExpiry)
                {
                    var result = currentCache.CachedLanguage.Value;
                    tcs.SetResult(result);
                    return result;
                }

                var userRole = await authService.GetMyRoleAsync();
                if (userRole?.EmployeeId == null)
                {
                    var result = Language.English;
                    tcs.SetResult(result);
                    return result;
                }

                var language = await GetUserPreferredLanguageAsync(userRole.EmployeeId);
                cache = new CacheData(language, DateTime.UtcNow);
                tcs.SetResult(language);
                return language;
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
                return Language.English; // Fallback
            }
            finally
            {
                // Clear the refresh task atomically
                Interlocked.CompareExchange(ref refreshTaskSource, null, tcs);
            }
        }
        else
        {
            // Another thread is refreshing, wait for their result
            try
            {
                return await existingTcs.Task;
            }
            catch
            {
                return Language.English; // Fallback if refresh failed
            }
        }
    }

    public async Task<Language> GetUserPreferredLanguageAsync(Guid userId)
    {
        try
        {
            // Query the server API for current language preference
            var response = await httpClient.GetAsync($"/q/api/v1/employees/{userId}/language");
            if (response.IsSuccessStatusCode)
            {
                var languageString = await response.Content.ReadAsStringAsync();
                if (Enum.TryParse<Language>(languageString.Trim('"'), out var language))
                {
                    Console.WriteLine($"[ClientLanguageContext] Retrieved language {language} from server for user {userId}");
                    return language;
                }
            }

            Console.WriteLine($"[ClientLanguageContext] Using default English for user {userId} (API call failed)");
            return Language.English; // Fallback
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ClientLanguageContext] Exception getting language for user {userId}: {ex.Message}");
            return Language.English; // Always fallback
        }
    }

    public async Task SetUserPreferredLanguageAsync(Guid userId, Language language)
    {
        try
        {
            // Use CommandClient for CQRS - commands should go to CommandApi, not QueryApi
            var commandClient = httpClientFactory.CreateClient("CommandClient");

            // Create proper request object to match CommandApi endpoint expectation
            // The CommandApi expects LanguageDto enum value (0=English, 1=German)
            var requestPayload = new { Language = (int)language };
            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await commandClient.PostAsync($"/c/api/v1/employees/{userId}/language", content);
            if (response.IsSuccessStatusCode)
            {
                // Update cache immediately for responsive UI
                cache = new CacheData(language, DateTime.UtcNow);

                Console.WriteLine($"[ClientLanguageContext] Successfully saved language {language} for user {userId}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ClientLanguageContext] Failed to save language {language} for user {userId}: {response.StatusCode} - {errorContent}");
                throw new Exception($"Failed to save language preference: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ClientLanguageContext] Error setting language preference: {ex.Message}");
            throw; // Re-throw to let the caller handle the error
        }
    }

    /// <summary>
    /// Gets the current language synchronously from cache, or English as fallback.
    /// For performance-optimized components that need immediate access to language.
    /// </summary>
    public Language CurrentLanguage => cache.CachedLanguage ?? Language.English;

    /// <summary>
    /// Sets the current language in cache immediately for performance optimization.
    /// Use this for immediate UI language switching before async persistence.
    /// Thread-safe implementation for singleton usage.
    /// </summary>
    public void SetCurrentLanguage(Language language)
    {
        cache = new CacheData(language, DateTime.UtcNow); // Atomic reference update
    }

    /// <summary>
    /// Clears the cached language, forcing a refresh on next access.
    /// Useful when user logs in/out or when language is changed by another part of the application.
    /// Thread-safe implementation for singleton usage.
    /// </summary>
    public void ClearCache()
    {
        cache = new CacheData(null, DateTime.MinValue); // Atomic reference update
    }
}

/// <summary>
/// Interface for client-side language context.
/// Matches the server-side interface but adapted for client needs.
/// </summary>
public interface ILanguageContext
{
    /// <summary>
    /// Gets the current language synchronously from cache, or English as fallback.
    /// For performance-optimized components that need immediate access to language.
    /// </summary>
    Language CurrentLanguage { get; }

    /// <summary>
    /// Sets the current language in cache immediately for performance optimization.
    /// Use this for immediate UI language switching before async persistence.
    /// </summary>
    void SetCurrentLanguage(Language language);

    Task<Language> GetCurrentLanguageAsync();
    Task<Language> GetUserPreferredLanguageAsync(Guid userId);
    Task SetUserPreferredLanguageAsync(Guid userId, Language language);
}