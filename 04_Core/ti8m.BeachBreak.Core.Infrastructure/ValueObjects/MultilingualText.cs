using System.Text.Json.Serialization;

namespace ti8m.BeachBreak.Core.Infrastructure.ValueObjects;

public class MultilingualText
{
    [JsonPropertyName("en")]
    public string English { get; set; } = string.Empty;

    [JsonPropertyName("de")]
    public string German { get; set; } = string.Empty;

    public MultilingualText() { }

    public MultilingualText(string english, string german = "")
    {
        English = english;
        German = german;
    }

    public string GetText(string languageCode = "en")
    {
        return languageCode.ToLower() switch
        {
            "de" or "de-de" or "de-ch" or "de-at" => !string.IsNullOrEmpty(German) ? German : English,
            _ => !string.IsNullOrEmpty(English) ? English : German
        };
    }

    public bool IsEmpty => string.IsNullOrEmpty(English) && string.IsNullOrEmpty(German);

    public bool HasBothLanguages => !string.IsNullOrEmpty(English) && !string.IsNullOrEmpty(German);

    public bool HasEnglish => !string.IsNullOrEmpty(English);

    public bool HasGerman => !string.IsNullOrEmpty(German);

    public static implicit operator MultilingualText(string text)
    {
        return new MultilingualText(text);
    }

    public static implicit operator string(MultilingualText text)
    {
        return text?.GetText() ?? string.Empty;
    }

    public override string ToString()
    {
        return GetText();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not MultilingualText other) return false;
        return English == other.English && German == other.German;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(English, German);
    }
}