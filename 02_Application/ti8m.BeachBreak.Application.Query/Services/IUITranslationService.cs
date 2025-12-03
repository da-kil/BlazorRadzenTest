using ti8m.BeachBreak.Application.Query.Models;

namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Service for managing UI translations and providing localized text.
/// Supports caching for performance and fallback to English when translations are missing.
/// </summary>
public interface IUITranslationService
{
    /// <summary>
    /// Gets translated text for the specified key and language.
    /// Returns English as fallback if the German translation is not available.
    /// </summary>
    /// <param name="key">The translation key (e.g., "common.buttons.save")</param>
    /// <param name="language">The target language</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The translated text or English fallback</returns>
    Task<string> GetTextAsync(string key, Language language, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all translations for administrative purposes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All UI translations</returns>
    Task<IList<UITranslation>> GetAllTranslationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets translations filtered by category.
    /// </summary>
    /// <param name="category">The category to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Translations in the specified category</returns>
    Task<IList<UITranslation>> GetTranslationsByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates a translation.
    /// </summary>
    /// <param name="key">The translation key</param>
    /// <param name="german">German text</param>
    /// <param name="english">English text</param>
    /// <param name="category">Optional category for organization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created or updated translation</returns>
    Task<UITranslation> UpsertTranslationAsync(string key, string german, string english, string? category = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a translation by key.
    /// </summary>
    /// <param name="key">The translation key to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the translation was deleted, false if it didn't exist</returns>
    Task<bool> DeleteTranslationAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a translation exists for the given key.
    /// </summary>
    /// <param name="key">The translation key to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the translation exists</returns>
    Task<bool> TranslationExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds the database with initial translation data if no translations exist.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of translations seeded</returns>
    Task<int> SeedInitialTranslationsAsync(CancellationToken cancellationToken = default);
}