using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.Core.Domain;

public static class QuestionnaireProcessTypeExtensions
{
    /// <summary>
    /// Returns whether this process type requires manager review.
    /// </summary>
    public static bool RequiresManagerReview(this QuestionnaireProcessType processType)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => true,
            QuestionnaireProcessType.Survey => false,
            _ => throw new ArgumentOutOfRangeException(nameof(processType))
        };
    }

    /// <summary>
    /// Returns whether the specified question type is allowed for this process type.
    /// </summary>
    public static bool IsQuestionTypeAllowed(this QuestionnaireProcessType processType, QuestionType questionType)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => true, // All types allowed
            QuestionnaireProcessType.Survey => questionType is QuestionType.Assessment
                                                            or QuestionType.TextQuestion,
            _ => throw new ArgumentOutOfRangeException(nameof(processType))
        };
    }

    /// <summary>
    /// Returns the allowed question types for this process type.
    /// </summary>
    public static IReadOnlyList<QuestionType> GetAllowedQuestionTypes(this QuestionnaireProcessType processType)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => new[]
            {
                QuestionType.Assessment,
                QuestionType.TextQuestion,
                QuestionType.Goal,
                QuestionType.EmployeeFeedback
            },
            QuestionnaireProcessType.Survey => new[]
            {
                QuestionType.Assessment,
                QuestionType.TextQuestion
            },
            _ => throw new ArgumentOutOfRangeException(nameof(processType))
        };
    }

    /// <summary>
    /// Returns whether the specified completion role is allowed for this process type.
    /// </summary>
    public static bool IsCompletionRoleAllowed(this QuestionnaireProcessType processType, CompletionRole role)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => true, // Employee, Manager, Both all allowed
            QuestionnaireProcessType.Survey => role == CompletionRole.Employee,
            _ => throw new ArgumentOutOfRangeException(nameof(processType))
        };
    }

    /// <summary>
    /// Returns the allowed completion roles for this process type.
    /// </summary>
    public static IReadOnlyList<CompletionRole> GetAllowedCompletionRoles(this QuestionnaireProcessType processType)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => new[]
            {
                CompletionRole.Employee,
                CompletionRole.Manager,
                CompletionRole.Both
            },
            QuestionnaireProcessType.Survey => new[]
            {
                CompletionRole.Employee
            },
            _ => throw new ArgumentOutOfRangeException(nameof(processType))
        };
    }
}
