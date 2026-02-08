using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service for managing translation categories and category-based loading strategies.
/// </summary>
public interface ITranslationCategoryService
{
    /// <summary>
    /// Gets translation keys for the specified categories.
    /// </summary>
    Task<string[]> GetKeysByCategoriesAsync(params string[] categories);

    /// <summary>
    /// Gets translations for the specified categories in the requested language.
    /// Optimized for batch loading to reduce network transfer and memory usage.
    /// </summary>
    Task<Dictionary<string, string>> GetTranslationsByCategoriesAsync(Language language, params string[] categories);

    /// <summary>
    /// Gets all available translation categories.
    /// </summary>
    Task<string[]> GetAllCategoriesAsync();
}

/// <summary>
/// Translation category definitions for efficient loading strategies.
/// </summary>
public static class TranslationCategories
{
    /// <summary>
    /// Core UI elements - always loaded for every component.
    /// Includes navigation, buttons, common messages (~50 keys).
    /// </summary>
    public const string Core = "core";

    /// <summary>
    /// Navigation menu items and links (~30 keys).
    /// </summary>
    public const string Navigation = "navigation";

    /// <summary>
    /// Standard buttons (save, cancel, delete, edit, etc.) (~20 keys).
    /// </summary>
    public const string Buttons = "buttons";

    /// <summary>
    /// Notification messages (success, error, warning, info) (~15 keys).
    /// </summary>
    public const string Notifications = "notifications";

    /// <summary>
    /// Form validation messages (~25 keys).
    /// </summary>
    public const string Validation = "validation";

    /// <summary>
    /// Questionnaire-specific translations (~200 keys).
    /// </summary>
    public const string Questionnaires = "questionnaires";

    /// <summary>
    /// Employee and HR management (~100 keys).
    /// </summary>
    public const string Employees = "employees";

    /// <summary>
    /// Administration pages (~80 keys).
    /// </summary>
    public const string Administration = "administration";

    /// <summary>
    /// Form labels and field names (~150 keys).
    /// </summary>
    public const string Forms = "forms";

    /// <summary>
    /// Page titles and section headers (~60 keys).
    /// </summary>
    public const string Pages = "pages";

    /// <summary>
    /// Status and action labels (~80 keys).
    /// </summary>
    public const string Status = "status";

    /// <summary>
    /// Dialog and modal content (~90 keys).
    /// </summary>
    public const string Dialogs = "dialogs";

    /// <summary>
    /// Error and informational messages (~50 keys).
    /// </summary>
    public const string Messages = "messages";

    /// <summary>
    /// General purpose translations that don't fit other categories (~50 keys).
    /// </summary>
    public const string General = "general";

    /// <summary>
    /// Core categories that should be pre-loaded for every component.
    /// Total: ~95 keys (instead of 978).
    /// </summary>
    public static readonly string[] CoreCategories = new[]
    {
        Navigation,
        Buttons,
        Notifications,
        Validation
    };

    /// <summary>
    /// Common categories loaded for most UI components.
    /// Total: ~205 keys (still 75% reduction).
    /// </summary>
    public static readonly string[] CommonCategories = new[]
    {
        Navigation,
        Buttons,
        Notifications,
        Validation,
        Forms,
        Pages,
        Messages
    };

    /// <summary>
    /// Questionnaire-specific categories.
    /// Total: ~375 keys (used only by questionnaire components).
    /// </summary>
    public static readonly string[] QuestionnaireCategories = new[]
    {
        Navigation,
        Buttons,
        Notifications,
        Validation,
        Questionnaires,
        Forms,
        Status,
        Dialogs
    };

    /// <summary>
    /// Administration-specific categories.
    /// Total: ~300 keys (used only by admin components).
    /// </summary>
    public static readonly string[] AdminCategories = new[]
    {
        Navigation,
        Buttons,
        Notifications,
        Validation,
        Administration,
        Forms,
        Employees,
        Messages
    };
}