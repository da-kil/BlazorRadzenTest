using Blazored.LocalStorage;
using Radzen;

namespace ti8m.BeachBreak.Client.Services;

public class LocalStorageThemeService
{
    private readonly ILocalStorageService _localStorage;
    private readonly ThemeService _themeService;
    private const string THEME_KEY = "beachbreak-theme";
    private const string RTL_KEY = "beachbreak-rtl";
    private const string WCAG_KEY = "beachbreak-wcag";
    private const string DEFAULT_THEME = "material";

    public LocalStorageThemeService(ILocalStorageService localStorage, ThemeService themeService)
    {
        _localStorage = localStorage;
        _themeService = themeService;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var theme = await _localStorage.GetItemAsync<string>(THEME_KEY) ?? DEFAULT_THEME;
            var rtl = await _localStorage.GetItemAsync<bool?>(RTL_KEY) ?? false;
            var wcag = await _localStorage.GetItemAsync<bool?>(WCAG_KEY) ?? false;

            // Apply saved settings
            _themeService.SetTheme(theme);
            if (rtl)
            {
                _themeService.SetRightToLeft(true);
            }
            if (wcag)
            {
                _themeService.SetWcag(true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing theme: {ex.Message}");
            _themeService.SetTheme(DEFAULT_THEME);
        }
    }

    public async Task SetThemeAsync(string theme)
    {
        _themeService.SetTheme(theme);
        await _localStorage.SetItemAsync(THEME_KEY, theme);
    }

    public async Task SetRightToLeftAsync(bool rtl)
    {
        _themeService.SetRightToLeft(rtl);
        await _localStorage.SetItemAsync(RTL_KEY, rtl);
    }

    public async Task SetWcagAsync(bool wcag)
    {
        _themeService.SetWcag(wcag);
        await _localStorage.SetItemAsync(WCAG_KEY, wcag);
    }

    public string GetCurrentTheme() => _themeService.Theme ?? DEFAULT_THEME;
    public bool GetRightToLeft() => _themeService.RightToLeft ?? false;
    public bool GetWcag() => _themeService.Wcag ?? false;
}