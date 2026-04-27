namespace ti8m.BeachBreak.Client.Models;

public class BinaryItem
{
    public string Key { get; set; } = string.Empty;
    public string TitleEnglish { get; set; } = string.Empty;
    public string TitleGerman { get; set; } = string.Empty;
    public string? DescriptionEnglish { get; set; }
    public string? DescriptionGerman { get; set; }
    public string OptionALabelEnglish { get; set; } = "Yes";
    public string OptionALabelGerman { get; set; } = "Ja";
    public string OptionBLabelEnglish { get; set; } = "No";
    public string OptionBLabelGerman { get; set; } = "Nein";
    public bool IsRequired { get; set; }
    public int Order { get; set; }

    public BinaryItem() { }

    public BinaryItem(string key, int order)
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
