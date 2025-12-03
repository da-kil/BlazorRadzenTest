using ti8m.BeachBreak.CommandApi.Dto;
using ApplicationLanguage = ti8m.BeachBreak.Application.Command.Models.Language;

namespace ti8m.BeachBreak.CommandApi.Mappers;

/// <summary>
/// Provides explicit, type-safe mapping between API-layer LanguageDto and Application Language enums.
/// This ensures compile-time safety and prevents silent failures from enum value drift.
/// Follows Clean Architecture by mapping from Infrastructure layer to Application layer.
/// </summary>
public static class LanguageMapper
{
    /// <summary>
    /// Maps from API-layer LanguageDto to Application Language.
    /// Used when CommandApi receives DTO languages from requests and needs to create Application commands.
    /// </summary>
    public static ApplicationLanguage MapToApplication(LanguageDto dtoLanguage)
    {
        return dtoLanguage switch
        {
            LanguageDto.English => ApplicationLanguage.English,
            LanguageDto.German => ApplicationLanguage.German,
            _ => throw new ArgumentOutOfRangeException(nameof(dtoLanguage), dtoLanguage,
                $"Unknown LanguageDto: {dtoLanguage}")
        };
    }

    /// <summary>
    /// Maps from Application Language to API-layer LanguageDto.
    /// Used when CommandApi needs to return language values in responses.
    /// </summary>
    public static LanguageDto MapFromApplication(ApplicationLanguage applicationLanguage)
    {
        return applicationLanguage switch
        {
            ApplicationLanguage.English => LanguageDto.English,
            ApplicationLanguage.German => LanguageDto.German,
            _ => throw new ArgumentOutOfRangeException(nameof(applicationLanguage), applicationLanguage,
                $"Unknown Application Language: {applicationLanguage}")
        };
    }
}