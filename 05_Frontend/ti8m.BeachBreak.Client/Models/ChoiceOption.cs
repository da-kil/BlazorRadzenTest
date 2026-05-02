namespace ti8m.BeachBreak.Client.Models;

public class ChoiceOption
{
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

    public string Key { get; set; }
    public string LabelEnglish { get; set; }
    public string LabelGerman { get; set; }
    public string? DescriptionEnglish { get; set; }
    public string? DescriptionGerman { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }

    public string GetLocalizedLabel(string language)
    {
        return language == "de" ? LabelGerman : LabelEnglish;
    }

    public string GetLocalizedLabelWithFallback(string language)
    {
        var label = GetLocalizedLabel(language);
        return !string.IsNullOrWhiteSpace(label) ? label : LabelEnglish;
    }

    public string? GetLocalizedDescription(string language)
    {
        var desc = language == "de" ? DescriptionGerman : DescriptionEnglish;
        return !string.IsNullOrWhiteSpace(desc) ? desc : DescriptionEnglish;
    }
}
