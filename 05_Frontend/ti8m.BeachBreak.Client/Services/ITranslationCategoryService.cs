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
/// Categories match the actual categories in the translation data.
/// </summary>
public static class TranslationCategories
{
    // === Core Categories (actual categories from translation data) ===

    /// <summary>
    /// Action buttons and links
    /// </summary>
    public const string Actions = "actions";

    /// <summary>
    /// Assignment-related translations
    /// </summary>
    public const string Assignments = "assignments";

    /// <summary>
    /// Standard buttons (save, cancel, delete, edit, etc.)
    /// </summary>
    public const string Buttons = "buttons";

    /// <summary>
    /// Table column headers
    /// </summary>
    public const string Columns = "columns";

    /// <summary>
    /// Completion role labels
    /// </summary>
    public const string CompletionRoles = "completion-roles";

    /// <summary>
    /// Dashboard-specific translations
    /// </summary>
    public const string Dashboard = "dashboard";

    /// <summary>
    /// Dialog and modal content
    /// </summary>
    public const string Dialogs = "dialogs";

    /// <summary>
    /// Error messages
    /// </summary>
    public const string Errors = "errors";

    /// <summary>
    /// Feedback-related translations
    /// </summary>
    public const string Feedback = "feedback";

    /// <summary>
    /// Feedback source types
    /// </summary>
    public const string FeedbackSource = "feedback-source";

    /// <summary>
    /// Filter-related translations
    /// </summary>
    public const string Filters = "filters";

    /// <summary>
    /// Form-related translations
    /// </summary>
    public const string Forms = "forms";

    /// <summary>
    /// Goal-related translations
    /// </summary>
    public const string Goals = "goals";

    /// <summary>
    /// Help text and tooltips
    /// </summary>
    public const string Help = "help";

    /// <summary>
    /// Form labels and field names
    /// </summary>
    public const string Labels = "labels";

    /// <summary>
    /// Language-related translations
    /// </summary>
    public const string Language = "language";

    /// <summary>
    /// General messages
    /// </summary>
    public const string Messages = "messages";

    /// <summary>
    /// Navigation items (short form)
    /// </summary>
    public const string Nav = "nav";

    /// <summary>
    /// Navigation menu items and links
    /// </summary>
    public const string Navigation = "navigation";

    /// <summary>
    /// Notification messages
    /// </summary>
    public const string Notifications = "notifications";

    /// <summary>
    /// Page titles and section headers
    /// </summary>
    public const string Pages = "pages";

    /// <summary>
    /// Form placeholders and hints
    /// </summary>
    public const string Placeholders = "placeholders";

    /// <summary>
    /// Process type labels
    /// </summary>
    public const string ProcessTypes = "process-types";

    /// <summary>
    /// Question-related translations
    /// </summary>
    public const string Questions = "questions";

    /// <summary>
    /// Question type labels
    /// </summary>
    public const string QuestionTypes = "question-types";

    /// <summary>
    /// Rating-related translations
    /// </summary>
    public const string Rating = "rating";

    /// <summary>
    /// Rating scale labels
    /// </summary>
    public const string RatingScale = "rating-scale";

    /// <summary>
    /// Reopen functionality
    /// </summary>
    public const string Reopen = "reopen";

    /// <summary>
    /// Reopen reason descriptions
    /// </summary>
    public const string ReopenDescriptions = "reopen-descriptions";

    /// <summary>
    /// User role labels
    /// </summary>
    public const string Roles = "roles";

    /// <summary>
    /// Section headers and titles
    /// </summary>
    public const string Sections = "sections";

    /// <summary>
    /// Settings-related translations
    /// </summary>
    public const string Settings = "settings";

    /// <summary>
    /// Source type labels
    /// </summary>
    public const string SourceTypes = "source-types";

    /// <summary>
    /// Status labels and indicators
    /// </summary>
    public const string Status = "status";

    /// <summary>
    /// Step-by-step instructions
    /// </summary>
    public const string Steps = "steps";

    /// <summary>
    /// Tab labels
    /// </summary>
    public const string Tabs = "tabs";

    /// <summary>
    /// Template-related translations
    /// </summary>
    public const string Templates = "templates";

    /// <summary>
    /// Title and heading text
    /// </summary>
    public const string Titles = "titles";

    /// <summary>
    /// Tooltip text
    /// </summary>
    public const string Tooltips = "tooltips";

    /// <summary>
    /// State transition labels
    /// </summary>
    public const string Transitions = "transitions";

    /// <summary>
    /// Form validation messages
    /// </summary>
    public const string Validation = "validation";

    /// <summary>
    /// Warning messages
    /// </summary>
    public const string Warnings = "warnings";

    /// <summary>
    /// Workflow state labels
    /// </summary>
    public const string WorkflowStates = "workflow-states";

    // === Category Groups for Common Loading Patterns ===

    /// <summary>
    /// Core categories that should be pre-loaded for most components.
    /// Includes essential UI elements and common interactions.
    /// </summary>
    public static readonly string[] CoreCategories = new[]
    {
        Navigation,
        Buttons,
        Notifications,
        Validation,
        Messages,
        Errors
    };

    /// <summary>
    /// Common categories loaded for standard UI components.
    /// Includes forms, labels, and common user interactions.
    /// </summary>
    public static readonly string[] CommonCategories = new[]
    {
        Navigation,
        Buttons,
        Notifications,
        Validation,
        Labels,
        Placeholders,
        Forms,
        Pages,
        Messages,
        Dialogs,
        Actions
    };

    /// <summary>
    /// Comprehensive categories for complex components that need extensive translations.
    /// Includes questionnaire, feedback, and management functionality.
    /// </summary>
    public static readonly string[] ExtensiveCategories = new[]
    {
        Navigation,
        Buttons,
        Notifications,
        Validation,
        Labels,
        Placeholders,
        Forms,
        Pages,
        Messages,
        Dialogs,
        Actions,
        Columns,
        Status,
        Questions,
        Goals,
        Feedback,
        Assignments,
        WorkflowStates,
        Roles
    };

    /// <summary>
    /// Questionnaire-specific categories.
    /// Includes questionnaire, assignment, and workflow management functionality.
    /// </summary>
    public static readonly string[] QuestionnaireCategories = new[]
    {
        Navigation,
        Buttons,
        Notifications,
        Validation,
        Questions,
        QuestionTypes,
        Goals,
        Assignments,
        WorkflowStates,
        Forms,
        Status,
        Dialogs,
        Labels,
        Placeholders
    };

    /// <summary>
    /// Administration-specific categories.
    /// Includes settings, user management, and administrative functionality.
    /// </summary>
    public static readonly string[] AdminCategories = new[]
    {
        Navigation,
        Buttons,
        Notifications,
        Validation,
        Settings,
        Roles,
        Templates,
        Forms,
        Messages,
        Status,
        Actions,
        Labels,
        Placeholders
    };
}