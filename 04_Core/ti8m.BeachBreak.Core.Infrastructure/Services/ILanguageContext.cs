namespace ti8m.BeachBreak.Core.Infrastructure.Services;

/// <summary>
/// Service for managing language context and user language preferences.
/// Uses integer language codes to maintain Clean Architecture by avoiding Domain layer dependencies.
/// Language codes: 0=English, 1=German
/// </summary>
public interface ILanguageContext
{
    /// <summary>
    /// Gets the current language code for the user context.
    /// Returns 0 (English) as fallback if no preference is set.
    /// </summary>
    /// <returns>The current user's preferred language code (0=English, 1=German)</returns>
    Task<int> GetCurrentLanguageCodeAsync();

    /// <summary>
    /// Gets the preferred language code for a specific user.
    /// Returns 0 (English) as fallback if no preference is set.
    /// </summary>
    /// <param name="userId">The user ID to get language preference for</param>
    /// <returns>The user's preferred language code (0=English, 1=German)</returns>
    Task<int> GetUserPreferredLanguageCodeAsync(Guid userId);

    /// <summary>
    /// Sets the preferred language for a specific user.
    /// This will trigger domain events and update the user's preference.
    /// </summary>
    /// <param name="userId">The user ID to set language preference for</param>
    /// <param name="languageCode">The language code to set as preferred (0=English, 1=German)</param>
    /// <returns>Task representing the async operation</returns>
    Task SetUserPreferredLanguageCodeAsync(Guid userId, int languageCode);
}