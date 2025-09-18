using ti8m.BeachBreak.Core.Infrastructure.ValueObjects;

namespace ti8m.BeachBreak.Client.Services;

public interface ILanguageService
{
    string CurrentLanguage { get; }
    event Action<string>? LanguageChanged;
    void SetLanguage(string languageCode);
    string GetText(MultilingualText text);
    List<string> GetOptions(MultilingualOptions options);
}

public class LanguageService : ILanguageService
{
    private string _currentLanguage = "en";

    public string CurrentLanguage => _currentLanguage;

    public event Action<string>? LanguageChanged;

    public void SetLanguage(string languageCode)
    {
        var normalizedCode = NormalizeLanguageCode(languageCode);
        if (_currentLanguage != normalizedCode)
        {
            _currentLanguage = normalizedCode;
            LanguageChanged?.Invoke(_currentLanguage);
        }
    }

    public string GetText(MultilingualText text)
    {
        return text?.GetText(_currentLanguage) ?? string.Empty;
    }

    public List<string> GetOptions(MultilingualOptions options)
    {
        return options?.GetOptions(_currentLanguage) ?? new List<string>();
    }

    private static string NormalizeLanguageCode(string languageCode)
    {
        if (string.IsNullOrEmpty(languageCode))
            return "en";

        var normalized = languageCode.ToLower();
        return normalized.StartsWith("de") ? "de" : "en";
    }
}