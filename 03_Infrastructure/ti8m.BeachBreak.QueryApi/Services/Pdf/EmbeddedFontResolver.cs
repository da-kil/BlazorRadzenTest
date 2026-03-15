using System.Reflection;
using PdfSharp.Fonts;

namespace ti8m.BeachBreak.QueryApi.Services.Pdf;

/// <summary>
/// Resolves fonts from embedded resources in this assembly.
/// Font files are stored under the Fonts/ folder and compiled as EmbeddedResource.
/// </summary>
public class EmbeddedFontResolver : IFontResolver
{
    // Maps (familyName-variant) → embedded resource name
    private static readonly Dictionary<string, string> FontMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Arial-regular"]     = "arial.ttf",
        ["Arial-bold"]        = "arialbd.ttf",
        ["Arial-italic"]      = "ariali.ttf",
        ["Arial-bolditalic"]  = "arialbi.ttf",

        // Courier New is used internally by PDFsharp for error/fallback rendering
        ["Courier New-regular"]     = "cour.ttf",
        ["Courier New-bold"]        = "courbd.ttf",
        ["Courier New-italic"]      = "couri.ttf",
        ["Courier New-bolditalic"]  = "courbi.ttf",
    };

    private static readonly Assembly ThisAssembly = typeof(EmbeddedFontResolver).Assembly;

    // Cache loaded font bytes to avoid repeated stream reads
    private static readonly Dictionary<string, byte[]> Cache = new(StringComparer.OrdinalIgnoreCase);

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        var variant = (isBold, isItalic) switch
        {
            (true, true)   => "bolditalic",
            (true, false)  => "bold",
            (false, true)  => "italic",
            _              => "regular"
        };

        var key = $"{familyName}-{variant}";
        if (FontMap.ContainsKey(key))
            return new FontResolverInfo(key);

        // Fall back to regular if the specific variant isn't available
        var regularKey = $"{familyName}-regular";
        if (FontMap.ContainsKey(regularKey))
            return new FontResolverInfo(regularKey);

        return null;
    }

    public byte[]? GetFont(string faceName)
    {
        if (Cache.TryGetValue(faceName, out var cached))
            return cached;

        if (!FontMap.TryGetValue(faceName, out var fileName))
            return null;

        // Resource names follow the pattern: {AssemblyName}.Fonts.{filename}
        var resourceName = $"{ThisAssembly.GetName().Name}.Fonts.{fileName}";
        using var stream = ThisAssembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return null;

        var bytes = new byte[stream.Length];
        _ = stream.Read(bytes, 0, bytes.Length);

        Cache[faceName] = bytes;
        return bytes;
    }
}
