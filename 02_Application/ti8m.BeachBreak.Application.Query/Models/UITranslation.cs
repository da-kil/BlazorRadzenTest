namespace ti8m.BeachBreak.Application.Query.Models;

/// <summary>
/// Represents a UI text translation for different languages.
/// Used for translating interface text like buttons, labels, and messages.
/// </summary>
public class UITranslation
{
    /// <summary>
    /// Unique identifier for the translation record
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Translation key used to look up the text (e.g., "common.buttons.save")
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// German translation text
    /// </summary>
    public string German { get; set; } = string.Empty;

    /// <summary>
    /// English translation text
    /// </summary>
    public string English { get; set; } = string.Empty;

    /// <summary>
    /// Category for organizing translations (e.g., "navigation", "buttons", "validation")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// When this translation was created
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When this translation was last updated
    /// </summary>
    public DateTimeOffset? UpdatedDate { get; set; }

    /// <summary>
    /// Gets the translated text for the specified language.
    /// Returns the English text as fallback if the requested language is not available.
    /// </summary>
    /// <param name="language">The language to get the text for</param>
    /// <returns>The translated text</returns>
    public string GetText(Language language)
    {
        return language switch
        {
            Language.German => !string.IsNullOrWhiteSpace(German) ? German : English,
            Language.English => English,
            _ => English
        };
    }
}