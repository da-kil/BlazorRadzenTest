using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service for managing translation categories and category-based loading strategies.
/// Provides optimized translation loading to reduce network transfer from 978 keys to 50-200 keys per component.
/// </summary>
public class TranslationCategoryService : ITranslationCategoryService
{
    private readonly IUITranslationService translationService;

    public TranslationCategoryService(IUITranslationService translationService)
    {
        this.translationService = translationService;
    }

    /// <summary>
    /// Gets translation keys for the specified categories.
    /// </summary>
    public async Task<string[]> GetKeysByCategoriesAsync(params string[] categories)
    {
        if (categories == null || categories.Length == 0)
            return Array.Empty<string>();

        var allKeys = new List<string>();

        // Load translations for each category
        foreach (var category in categories)
        {
            var categoryTranslations = await translationService.GetTranslationsByCategoryAsync(category);
            allKeys.AddRange(categoryTranslations.Select(t => t.Key));
        }

        return allKeys.Distinct().ToArray();
    }

    /// <summary>
    /// Gets translations for the specified categories in the requested language.
    /// Optimized for batch loading to reduce network transfer and memory usage.
    /// </summary>
    public async Task<Dictionary<string, string>> GetTranslationsByCategoriesAsync(Language language, params string[] categories)
    {
        if (categories == null || categories.Length == 0)
            return new Dictionary<string, string>();

        var keys = await GetKeysByCategoriesAsync(categories);
        return await translationService.GetTranslationsAsync(keys, language);
    }

    /// <summary>
    /// Gets all available translation categories from the current translation data.
    /// </summary>
    public async Task<string[]> GetAllCategoriesAsync()
    {
        var allTranslations = await translationService.GetAllTranslationsAsync();
        return allTranslations
            .Select(t => t.Category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .OrderBy(c => c)
            .ToArray();
    }
}