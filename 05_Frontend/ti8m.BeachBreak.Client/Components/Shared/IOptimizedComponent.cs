namespace ti8m.BeachBreak.Client.Components.Shared;

/// <summary>
/// Lightweight interface for components that want optimization features without inheritance.
/// Use this with composition services for maximum flexibility.
/// </summary>
public interface IOptimizedComponent : IDisposable
{
    /// <summary>
    /// Controls whether the component should perform parameter optimization.
    /// </summary>
    bool EnableParameterOptimization { get; }

    /// <summary>
    /// Controls whether performance monitoring is enabled for this component.
    /// </summary>
    bool EnablePerformanceMonitoring { get; }

    /// <summary>
    /// The name used for performance tracking and error handling.
    /// </summary>
    string ComponentName { get; }

    /// <summary>
    /// Method to provide custom state change detection.
    /// </summary>
    bool HasStateChanged();
}

/// <summary>
/// Lightweight interface for translatable components.
/// Use this with ITranslationCategoryService for category-based translation loading.
/// </summary>
public interface ITranslatableComponent : IOptimizedComponent
{
    /// <summary>
    /// Specifies which translation categories this component requires.
    /// Return the appropriate category array for your component's needs.
    /// </summary>
    string[] GetRequiredTranslationCategories();

    /// <summary>
    /// Synchronous translation lookup method.
    /// Should return translated text or key as fallback.
    /// </summary>
    string T(string key);

    /// <summary>
    /// Updates the component language and reloads translations.
    /// </summary>
    Task UpdateLanguageAsync(object newLanguage);

    /// <summary>
    /// Refreshes the component's translation cache.
    /// </summary>
    Task RefreshTranslationsAsync();
}