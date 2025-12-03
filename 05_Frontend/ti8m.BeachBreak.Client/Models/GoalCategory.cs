namespace ti8m.BeachBreak.Client.Models;

public class GoalCategory
{
    // Bilingual content properties - matching QueryApi DTO naming
    public string TitleEnglish { get; set; } = string.Empty;
    public string TitleGerman { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;
    public string DescriptionGerman { get; set; } = string.Empty;

    // Legacy properties for backward compatibility - will be removed after migration
    [Obsolete("Use TitleEnglish/TitleGerman instead")]
    public string Title { get; set; } = string.Empty;
    [Obsolete("Use DescriptionEnglish/DescriptionGerman instead")]
    public string Description { get; set; } = string.Empty;

    public bool IsRequired { get; set; } = false;
    public int Order { get; set; }

    // Helper methods for language-aware content display
    public string GetLocalizedTitle(Language language)
    {
        return language == Language.German ? TitleGerman : TitleEnglish;
    }

    public string GetLocalizedDescription(Language language)
    {
        return language == Language.German ? DescriptionGerman : DescriptionEnglish;
    }

    // Helper method to fallback to English if German is empty
    public string GetLocalizedTitleWithFallback(Language language)
    {
        var localized = GetLocalizedTitle(language);
        return !string.IsNullOrWhiteSpace(localized) ? localized : TitleEnglish;
    }

    public string GetLocalizedDescriptionWithFallback(Language language)
    {
        var localized = GetLocalizedDescription(language);
        return !string.IsNullOrWhiteSpace(localized) ? localized : DescriptionEnglish;
    }
}