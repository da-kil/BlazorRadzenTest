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
    /// Indicates whether this text section must be filled for the question to be considered complete.
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Display order of this text section within the question.
    /// </summary>
    public int Order { get; set; }

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
}
