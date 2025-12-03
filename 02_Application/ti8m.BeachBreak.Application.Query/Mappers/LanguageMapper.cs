using ti8m.BeachBreak.Application.Query.Models;
using DomainLanguage = ti8m.BeachBreak.Domain.Language;

namespace ti8m.BeachBreak.Application.Query.Mappers;

/// <summary>
/// Provides explicit, type-safe mapping between Domain and Application.Query Language enums.
/// This ensures compile-time safety and prevents silent failures from enum value drift.
/// </summary>
public static class LanguageMapper
{
    /// <summary>
    /// Maps from Domain Language to Application.Query Language.
    /// Provides compile-time safety and explicit error handling.
    /// </summary>
    public static Language MapFromDomain(DomainLanguage domainLanguage)
    {
        return domainLanguage switch
        {
            DomainLanguage.English => Language.English,
            DomainLanguage.German => Language.German,
            _ => throw new ArgumentOutOfRangeException(nameof(domainLanguage), domainLanguage,
                $"Unknown Domain Language: {domainLanguage}")
        };
    }

    /// <summary>
    /// Maps from Application.Query Language to Domain Language.
    /// Used when Query layer needs to interact with Domain operations.
    /// </summary>
    public static DomainLanguage MapToDomain(Language queryLanguage)
    {
        return queryLanguage switch
        {
            Language.English => DomainLanguage.English,
            Language.German => DomainLanguage.German,
            _ => throw new ArgumentOutOfRangeException(nameof(queryLanguage), queryLanguage,
                $"Unknown Query Language: {queryLanguage}")
        };
    }

    /// <summary>
    /// Maps from integer language code to Application.Query Language.
    /// Used when Core.Infrastructure returns language codes instead of Domain enums.
    /// Maintains Clean Architecture by eliminating Core.Infrastructure → Domain dependencies.
    /// </summary>
    public static Language FromLanguageCode(int languageCode)
    {
        return languageCode switch
        {
            0 => Language.English,
            1 => Language.German,
            _ => Language.English // Default fallback to English for unknown codes
        };
    }

    /// <summary>
    /// Maps from Application.Query Language to integer language code.
    /// Used when Query layer needs to send language codes to Core.Infrastructure.
    /// </summary>
    public static int ToLanguageCode(Language queryLanguage)
    {
        return queryLanguage switch
        {
            Language.English => 0,
            Language.German => 1,
            _ => 0 // Default fallback to English code
        };
    }
}