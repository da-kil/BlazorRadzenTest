using System.Text.Json.Serialization;

namespace ti8m.BeachBreak.Core.Infrastructure.ValueObjects;

public class MultilingualOptions
{
    [JsonPropertyName("en")]
    public List<string> English { get; set; } = new();

    [JsonPropertyName("de")]
    public List<string> German { get; set; } = new();

    public MultilingualOptions() { }

    public MultilingualOptions(List<string> english, List<string>? german = null)
    {
        English = english ?? new List<string>();
        German = german ?? new List<string>();
    }

    public List<string> GetOptions(string languageCode = "en")
    {
        return languageCode.ToLower() switch
        {
            "de" or "de-de" or "de-ch" or "de-at" => German.Any() ? German : English,
            _ => English.Any() ? English : German
        };
    }

    public bool IsEmpty => !English.Any() && !German.Any();

    public bool HasBothLanguages => English.Any() && German.Any();

    public bool HasEnglish => English.Any();

    public bool HasGerman => German.Any();

    public static implicit operator MultilingualOptions(List<string> options)
    {
        return new MultilingualOptions(options);
    }

    public static implicit operator List<string>(MultilingualOptions options)
    {
        return options?.GetOptions() ?? new List<string>();
    }

    public void AddOption(string englishOption, string? germanOption = null)
    {
        English.Add(englishOption);
        if (!string.IsNullOrEmpty(germanOption))
        {
            German.Add(germanOption);
        }
    }

    public void RemoveOption(int index)
    {
        if (index >= 0 && index < English.Count)
        {
            English.RemoveAt(index);
        }
        if (index >= 0 && index < German.Count)
        {
            German.RemoveAt(index);
        }
    }

    public void UpdateOption(int index, string englishOption, string? germanOption = null)
    {
        if (index >= 0 && index < English.Count)
        {
            English[index] = englishOption;
        }

        if (!string.IsNullOrEmpty(germanOption))
        {
            // Ensure German list is same size as English
            while (German.Count <= index)
            {
                German.Add(string.Empty);
            }
            German[index] = germanOption;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is not MultilingualOptions other) return false;
        return English.SequenceEqual(other.English) && German.SequenceEqual(other.German);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(English, German);
    }
}