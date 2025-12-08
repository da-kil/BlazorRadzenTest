namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

/// <summary>
/// Defines a text input section within a text question.
/// Each section represents a distinct text area where users can provide responses.
/// </summary>
public class TextSectionDefinition
{
    /// <summary>
    /// English title of the text section.
    /// </summary>
    public string TitleEnglish { get; set; } = string.Empty;

    /// <summary>
    /// German title of the text section.
    /// </summary>
    public string TitleGerman { get; set; } = string.Empty;

    /// <summary>
    /// English description providing context for this text section.
    /// </summary>
    public string DescriptionEnglish { get; set; } = string.Empty;

    /// <summary>
    /// German description providing context for this text section.
    /// </summary>
    public string DescriptionGerman { get; set; } = string.Empty;

    /// <summary>
    /// English placeholder text shown in the empty text area.
    /// </summary>
    public string PlaceholderEnglish { get; set; } = string.Empty;

    /// <summary>
    /// German placeholder text shown in the empty text area.
    /// </summary>
    public string PlaceholderGerman { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this text section must be filled for the question to be considered complete.
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Display order of this text section within the question.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Number of rows to display in the text area (affects UI height).
    /// </summary>
    public int Rows { get; set; } = 4;

    /// <summary>
    /// Gets the localized title based on the specified language.
    /// </summary>
    public string GetLocalizedTitle(Language language)
    {
        return language == Language.German ? TitleGerman : TitleEnglish;
    }

    /// <summary>
    /// Gets the localized description based on the specified language.
    /// </summary>
    public string GetLocalizedDescription(Language language)
    {
        return language == Language.German ? DescriptionGerman : DescriptionEnglish;
    }

    /// <summary>
    /// Gets the localized placeholder based on the specified language.
    /// </summary>
    public string GetLocalizedPlaceholder(Language language)
    {
        return language == Language.German ? PlaceholderGerman : PlaceholderEnglish;
    }

    /// <summary>
    /// Gets the localized title with fallback to English if German is not available.
    /// </summary>
    public string GetLocalizedTitleWithFallback(Language language)
    {
        var localized = GetLocalizedTitle(language);
        return !string.IsNullOrWhiteSpace(localized) ? localized : TitleEnglish;
    }

    /// <summary>
    /// Gets the localized description with fallback to English if German is not available.
    /// </summary>
    public string GetLocalizedDescriptionWithFallback(Language language)
    {
        var localized = GetLocalizedDescription(language);
        return !string.IsNullOrWhiteSpace(localized) ? localized : DescriptionEnglish;
    }
}
