using System.Text.Json;
using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Services;

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
    private Language? _cachedLanguage;
    private readonly TimeSpan cacheExpiry = TimeSpan.FromMinutes(10);
    private DateTime _lastCacheUpdate = DateTime.MinValue;

    public ClientLanguageContext(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        this.httpClient = httpClientFactory.CreateClient("QueryClient");
        this.httpClientFactory = httpClientFactory;
        this.authService = authService;
    }

    public async Task<Language> GetCurrentLanguageAsync()
    {
        // Return cached language if still valid
        if (_cachedLanguage.HasValue && DateTime.UtcNow - _lastCacheUpdate < cacheExpiry)
        {
            return _cachedLanguage.Value;
        }

        try
        {
            var userRole = await authService.GetMyRoleAsync();
            if (userRole?.EmployeeId == null)
            {
                return Language.English; // Default for non-authenticated users
            }

            var language = await GetUserPreferredLanguageAsync(userRole.EmployeeId);
            _cachedLanguage = language;
            _lastCacheUpdate = DateTime.UtcNow;
            return language;
        }
        catch
        {
            // Fallback to English if any error occurs
            return Language.English;
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
                _cachedLanguage = language;
                _lastCacheUpdate = DateTime.UtcNow;

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
    public Language CurrentLanguage => _cachedLanguage ?? Language.English;

    /// <summary>
    /// Sets the current language in cache immediately for performance optimization.
    /// Use this for immediate UI language switching before async persistence.
    /// </summary>
    public void SetCurrentLanguage(Language language)
    {
        _cachedLanguage = language;
        _lastCacheUpdate = DateTime.UtcNow;
    }

    /// <summary>
    /// Clears the cached language, forcing a refresh on next access.
    /// Useful when user logs in/out or when language is changed by another part of the application.
    /// </summary>
    public void ClearCache()
    {
        _cachedLanguage = null;
        _lastCacheUpdate = DateTime.MinValue;
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