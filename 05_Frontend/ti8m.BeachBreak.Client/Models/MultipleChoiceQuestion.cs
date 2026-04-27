namespace ti8m.BeachBreak.Client.Models;

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

    public string GetLocalizedTitle(string language) =>
        language == "de" ? TitleGerman : TitleEnglish;

    public string GetLocalizedTitleWithFallback(string language)
    {
        var localized = GetLocalizedTitle(language);
        return !string.IsNullOrWhiteSpace(localized) ? localized : TitleEnglish;
    }

    public string? GetLocalizedDescription(string language) =>
        language == "de" ? DescriptionGerman : DescriptionEnglish;
}
