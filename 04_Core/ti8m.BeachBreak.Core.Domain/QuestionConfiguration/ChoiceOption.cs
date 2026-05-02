namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

/// <summary>
/// Represents a selectable option within a multiple choice question.
/// </summary>
public class ChoiceOption
{
    public string Key { get; set; }
    public string LabelEnglish { get; set; }
    public string LabelGerman { get; set; }
    public string? DescriptionEnglish { get; set; }
    public string? DescriptionGerman { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }

    public ChoiceOption()
    {
        Key = string.Empty;
        LabelEnglish = string.Empty;
        LabelGerman = string.Empty;
    }

    public ChoiceOption(string key, string labelEnglish, string labelGerman, int order)
    {
        Key = key;
        LabelEnglish = labelEnglish;
        LabelGerman = labelGerman;
        Order = order;
    }

    public string GetLocalizedLabel(Language language)
    {
        return language == Language.German ? LabelGerman : LabelEnglish;
    }

    public string GetLocalizedLabelWithFallback(Language language)
    {
        var label = GetLocalizedLabel(language);
        return !string.IsNullOrWhiteSpace(label) ? label : LabelEnglish;
    }
}
