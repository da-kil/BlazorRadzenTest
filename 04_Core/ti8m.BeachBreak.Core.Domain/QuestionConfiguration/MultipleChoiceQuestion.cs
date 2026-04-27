namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

public class MultipleChoiceQuestion
{
    public string Key { get; set; } = string.Empty;
    public string TitleEnglish { get; set; } = string.Empty;
    public string TitleGerman { get; set; } = string.Empty;
    public string? DescriptionEnglish { get; set; }
    public string? DescriptionGerman { get; set; }
    public bool IsRequired => MinSelections > 0;
    public int Order { get; set; }
    public List<ChoiceOption> Choices { get; set; } = new();
    public int MinSelections { get; set; } = 1;
    public int MaxSelections { get; set; } = 1;

    public MultipleChoiceQuestion() { }

    public MultipleChoiceQuestion(string key, int order)
    {
        Key = key;
        Order = order;
    }

    public string GetLocalizedTitle(Language language) =>
        language == Language.German ? TitleGerman : TitleEnglish;

    public string GetLocalizedTitleWithFallback(Language language)
    {
        var localized = GetLocalizedTitle(language);
        return !string.IsNullOrWhiteSpace(localized) ? localized : TitleEnglish;
    }

    public string? GetLocalizedDescription(Language language) =>
        language == Language.German ? DescriptionGerman : DescriptionEnglish;
}
