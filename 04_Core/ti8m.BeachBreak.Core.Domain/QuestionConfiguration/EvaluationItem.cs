namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

/// <summary>
/// Represents an evaluation item within an assessment question.
/// Each item defines a criterion that will be rated by the user.
/// Renamed from CompetencyDefinition to better reflect domain terminology.
/// </summary>
public class EvaluationItem
{
    /// <summary>
    /// Unique identifier for this evaluation item within the question.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// English title of the evaluation criterion.
    /// </summary>
    public string TitleEnglish { get; set; }

    /// <summary>
    /// German title of the evaluation criterion.
    /// </summary>
    public string TitleGerman { get; set; }

    /// <summary>
    /// English description providing context for this evaluation.
    /// </summary>
    public string DescriptionEnglish { get; set; }

    /// <summary>
    /// German description providing context for this evaluation.
    /// </summary>
    public string DescriptionGerman { get; set; }

    /// <summary>
    /// Indicates whether this evaluation item must be rated for the question to be considered complete.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Display order of this evaluation item within the question.
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

/// <summary>
/// Supported languages for multilingual content.
/// </summary>
public enum Language
{
    English = 0,
    German = 1
}
