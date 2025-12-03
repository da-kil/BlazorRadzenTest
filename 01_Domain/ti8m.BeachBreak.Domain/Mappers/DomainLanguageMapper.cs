namespace ti8m.BeachBreak.Domain.Mappers;

/// <summary>
/// Mapper for converting between Domain.Language enum and integer language codes.
/// Maintains Clean Architecture by allowing Core.Infrastructure to use integers
/// while Domain layer handles the enum conversion internally.
/// </summary>
public static class DomainLanguageMapper
{
    /// <summary>
    /// Converts integer language code to Domain.Language enum.
    /// </summary>
    /// <param name="code">Language code (0=English, 1=German)</param>
    /// <returns>Corresponding Domain.Language enum value</returns>
    public static Language FromLanguageCode(int code) => code switch
    {
        0 => Language.English,
        1 => Language.German,
        _ => Language.English // Default fallback
    };

    /// <summary>
    /// Converts Domain.Language enum to integer language code.
    /// </summary>
    /// <param name="language">Domain.Language enum value</param>
    /// <returns>Corresponding integer code</returns>
    public static int ToLanguageCode(Language language) => (int)language;
}