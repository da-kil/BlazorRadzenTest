using Microsoft.AspNetCore.Components;
using Radzen;
using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Services;

namespace ti8m.BeachBreak.Client.Components.Shared;

/// <summary>
/// Performance-optimized base class for translatable components.
/// Inherits all performance optimizations from OptimizedComponentBase and adds
/// high-performance translation support with pre-loading and caching.
///
/// ELIMINATES Task.Run performance issues by pre-loading translations in OnInitializedAsync()
/// and providing synchronous T() method access during render.
/// </summary>
public abstract class OptimizedTranslatableComponentBase : OptimizedComponentBase
{
    [Inject] protected IUITranslationService TranslationService { get; set; } = default!;
    [Inject] protected ILanguageContext LanguageContext { get; set; } = default!;
    [Inject] protected NotificationService NotificationService { get; set; } = default!;

    private Dictionary<string, string> translations = new();
    private bool translationsLoaded = false;

    /// <summary>
    /// Override this to specify additional translation keys to pre-load.
    /// Common UI keys (buttons, navigation, validation) are always loaded.
    /// </summary>
    protected virtual Task<string[]> GetRequiredTranslationKeysAsync()
    {
        return Task.FromResult(Array.Empty<string>());
    }

    /// <summary>
    /// Gets the current language for this component.
    /// </summary>
    protected Language CurrentLanguage => LanguageContext.CurrentLanguage;

    /// <summary>
    /// High-performance synchronous translation lookup.
    /// NO Task.Run, NO async, NO UI thread blocking.
    /// Returns translation text or key as fallback.
    /// </summary>
    protected string T(string key)
    {
        return translations.TryGetValue(key, out var value) ? value : key;
    }

    /// <summary>
    /// Async translation method for code-behind scenarios where async is acceptable.
    /// </summary>
    protected async Task<string> TAsync(string key)
    {
        return await TranslationService.GetTextAsync(key, CurrentLanguage);
    }

    /// <summary>
    /// Updates the component language and reloads translations.
    /// Call this when the user switches language.
    /// </summary>
    protected async Task UpdateLanguageAsync(Language newLanguage)
    {
        LanguageContext.SetCurrentLanguage(newLanguage);
        await LoadTranslationsAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Refreshes the component's translation cache by clearing and reloading all translations.
    /// Call this when the translation cache has been cleared on the backend.
    /// </summary>
    public async Task RefreshTranslationsAsync()
    {
        try
        {
            // Clear component cache
            translations.Clear();
            translationsLoaded = false;

            // Reload translations from service (which will now fetch fresh data from server)
            await LoadTranslationsAsync();

            // Trigger re-render to show updated translations
            await InvokeAsync(StateHasChanged);

            if (EnablePerformanceMonitoring)
            {
                Console.WriteLine($"[{PerformanceTrackingName}] Translation cache refreshed - {translations.Count} translations reloaded");
            }
        }
        catch (Exception ex)
        {
            await HandleComponentErrorAsync(ex, "RefreshTranslations");

            // Fallback: ensure component can still render
            translations = new Dictionary<string, string>();
            translationsLoaded = true;
        }
    }

    /// <summary>
    /// Pre-loads all translations during component initialization.
    /// This follows the codebase's established pattern: async initialization → synchronous access.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync(); // Call OptimizedComponentBase initialization

        await ExecuteSafelyAsync(async () =>
        {
            // Ensure language is loaded in cache first
            await LanguageContext.GetCurrentLanguageAsync();

            // Then load translations for that language
            await LoadTranslationsAsync();
        }, "LoadTranslations");
    }

    /// <summary>
    /// Loads translations with aggressive pre-loading strategy.
    /// Loads ALL available translation keys to eliminate cache misses completely.
    /// </summary>
    private async Task LoadTranslationsAsync()
    {
        try
        {
            // Get ALL translation keys for complete pre-loading
            var allKeys = await TranslationService.GetAllTranslationKeysAsync();

            // Add any component-specific keys
            var componentKeys = await GetRequiredTranslationKeysAsync();
            var keysToLoad = allKeys.Concat(componentKeys).Distinct().ToArray();

            // Batch load all translations for current language
            translations = await TranslationService.GetTranslationsAsync(keysToLoad, CurrentLanguage);
            translationsLoaded = true;

            if (EnablePerformanceMonitoring)
            {
                Console.WriteLine($"[{PerformanceTrackingName}] Loaded {translations.Count} translations for {CurrentLanguage}");
            }
        }
        catch (Exception ex)
        {
            await HandleComponentErrorAsync(ex, "LoadTranslations");

            // Fallback: ensure component can still render with keys as fallbacks
            translations = new Dictionary<string, string>();
            translationsLoaded = true;
        }
    }

    /// <summary>
    /// Override state change detection to include translation loading state
    /// </summary>
    protected override bool HasStateChanged()
    {
        return base.HasStateChanged() || !translationsLoaded;
    }

    #region Translatable Notification Methods

    /// <summary>
    /// Show translated error notification
    /// </summary>
    protected virtual void ShowError(string message)
    {
        NotificationService.Notify(NotificationSeverity.Error, T("notifications.error"), message);
    }

    /// <summary>
    /// Show translated success notification
    /// </summary>
    protected virtual void ShowSuccess(string message)
    {
        NotificationService.Notify(NotificationSeverity.Success, T("notifications.success"), message);
    }

    /// <summary>
    /// Show translated info notification
    /// </summary>
    protected virtual void ShowInfo(string message)
    {
        NotificationService.Notify(NotificationSeverity.Info, T("notifications.information"), message);
    }

    /// <summary>
    /// Show translated warning notification
    /// </summary>
    protected virtual void ShowWarning(string message)
    {
        NotificationService.Notify(NotificationSeverity.Warning, T("notifications.warning"), message);
    }

    #endregion

    /// <summary>
    /// Override disposal to clean up translation cache
    /// </summary>
    public override void Dispose()
    {
        translations.Clear();
        base.Dispose();
    }
}