namespace ti8m.BeachBreak.Client.Models;

public class CompetencyDefinition
{
    // Parameterless constructor for JSON deserialization
    public CompetencyDefinition()
    {
        Key = string.Empty;
        TitleEnglish = string.Empty;
        TitleGerman = string.Empty;
        DescriptionEnglish = string.Empty;
        DescriptionGerman = string.Empty;
    }

    public CompetencyDefinition(string key, string titleEnglish, string descriptionEnglish, bool isRequired, int order = 0)
    {
        Key = key;
        TitleEnglish = titleEnglish;
        DescriptionEnglish = descriptionEnglish;
        IsRequired = isRequired;
        Order = order;
    }

    public string Key { get; set; }

    // Bilingual content properties - matching QueryApi DTO naming
    public string TitleEnglish { get; set; }
    public string TitleGerman { get; set; }
    public string DescriptionEnglish { get; set; }
    public string DescriptionGerman { get; set; }

    public bool IsRequired { get; set; }
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