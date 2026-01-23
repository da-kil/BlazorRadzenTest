using ti8m.BeachBreak.QueryApi.Dto;
using ti8m.BeachBreak.Core.Domain;
using DomainProcessType = ti8m.BeachBreak.Core.Domain.QuestionnaireProcessType;

namespace ti8m.BeachBreak.QueryApi.Mappers;

/// <summary>
/// Centralized enum conversion utilities for QueryApi controllers.
/// Eliminates duplicate enum mapping logic across controllers.
/// </summary>
public static class EnumConverter
{
    /// <summary>
    /// Maps string representation to CompletionRole enum.
    /// Case-insensitive matching with Employee as default fallback.
    /// </summary>
    /// <param name="completionRole">String representation of completion role</param>
    /// <returns>CompletionRole enum value</returns>
    public static CompletionRole MapToCompletionRole(string? completionRole)
    {
        return completionRole?.ToLower() switch
        {
            "manager" => CompletionRole.Manager,
            "both" => CompletionRole.Both,
            _ => CompletionRole.Employee
        };
    }

    /// <summary>
    /// Maps string representation to QuestionType enum.
    /// Case-insensitive matching with Assessment as default fallback.
    /// </summary>
    /// <param name="type">String representation of question type</param>
    /// <returns>QuestionType enum value</returns>
    public static Dto.QuestionType MapToQuestionType(string? type)
    {
        return type?.ToLower() switch
        {
            "textquestion" => Dto.QuestionType.TextQuestion,
            "goal" => Dto.QuestionType.Goal,
            "assessment" => Dto.QuestionType.Assessment,
            "employeefeedback" => Dto.QuestionType.EmployeeFeedback,
            _ => Dto.QuestionType.Assessment
        };
    }

    /// <summary>
    /// Maps Domain QuestionnaireProcessType to QueryApi DTO QuestionnaireProcessType.
    /// Throws ArgumentOutOfRangeException for unknown values to ensure type safety.
    /// </summary>
    /// <param name="domainProcessType">Domain process type</param>
    /// <returns>QueryApi DTO process type</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when domain process type is unknown</exception>
    public static Dto.QuestionnaireProcessType MapToProcessType(DomainProcessType domainProcessType)
    {
        return domainProcessType switch
        {
            DomainProcessType.PerformanceReview => Dto.QuestionnaireProcessType.PerformanceReview,
            DomainProcessType.Survey => Dto.QuestionnaireProcessType.Survey,
            _ => throw new ArgumentOutOfRangeException(
                nameof(domainProcessType),
                domainProcessType,
                $"Unknown QuestionnaireProcessType: {domainProcessType}")
        };
    }
}
