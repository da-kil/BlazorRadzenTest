using DtoProcessType = ti8m.BeachBreak.CommandApi.Dto.QuestionnaireProcessType;
using DomainProcessType = ti8m.BeachBreak.Core.Domain.QuestionnaireProcessType;

namespace ti8m.BeachBreak.CommandApi.Mappers;

/// <summary>
/// Provides explicit, type-safe mapping between different QuestionnaireProcessType enums for CommandApi controllers.
/// This ensures compile-time safety and prevents silent failures from enum value drift.
/// </summary>
public static class ProcessTypeMapper
{
    /// <summary>
    /// Maps from CommandApi DTO QuestionnaireProcessType to Core.Domain QuestionnaireProcessType.
    /// Used when CommandApi receives DTO from frontend and needs to create Commands.
    /// </summary>
    public static DomainProcessType MapToDomain(DtoProcessType dtoProcessType)
    {
        return dtoProcessType switch
        {
            DtoProcessType.PerformanceReview => DomainProcessType.PerformanceReview,
            DtoProcessType.Survey => DomainProcessType.Survey,
            _ => throw new ArgumentOutOfRangeException(nameof(dtoProcessType), dtoProcessType,
                $"Unknown DTO QuestionnaireProcessType: {dtoProcessType}")
        };
    }

    /// <summary>
    /// Maps from Core.Domain QuestionnaireProcessType to CommandApi DTO QuestionnaireProcessType.
    /// Used when CommandApi needs to return ProcessType in responses.
    /// </summary>
    public static DtoProcessType MapFromDomain(DomainProcessType domainProcessType)
    {
        return domainProcessType switch
        {
            DomainProcessType.PerformanceReview => DtoProcessType.PerformanceReview,
            DomainProcessType.Survey => DtoProcessType.Survey,
            _ => throw new ArgumentOutOfRangeException(nameof(domainProcessType), domainProcessType,
                $"Unknown Domain QuestionnaireProcessType: {domainProcessType}")
        };
    }
}
