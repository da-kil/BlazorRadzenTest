namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Extension methods and helper utilities for QuestionnaireProcessType.
/// Provides UI-specific helpers for rendering, validation, and business rules.
/// </summary>
public static class QuestionnaireProcessTypeHelper
{
    /// <summary>
    /// Determines if this process type requires manager review.
    /// </summary>
    public static bool RequiresManagerReview(this QuestionnaireProcessType processType)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => true,
            QuestionnaireProcessType.Survey => false,
            _ => throw new ArgumentOutOfRangeException(nameof(processType), processType, "Unknown process type")
        };
    }

    /// <summary>
    /// Checks if a specific question type is allowed for this process type.
    /// </summary>
    public static bool IsQuestionTypeAllowed(this QuestionnaireProcessType processType, QuestionType questionType)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => true, // All types allowed
            QuestionnaireProcessType.Survey => questionType switch
            {
                QuestionType.Assessment => true,
                QuestionType.TextQuestion => true,
                QuestionType.Goal => false, // Goals not allowed in surveys
                QuestionType.EmployeeFeedback => false, // Employee feedback not allowed in surveys
                _ => throw new ArgumentOutOfRangeException(nameof(questionType), questionType, "Unknown question type")
            },
            _ => throw new ArgumentOutOfRangeException(nameof(processType), processType, "Unknown process type")
        };
    }

    /// <summary>
    /// Checks if a specific completion role is allowed for this process type.
    /// </summary>
    public static bool IsCompletionRoleAllowed(this QuestionnaireProcessType processType, CompletionRole completionRole)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => true, // All roles allowed
            QuestionnaireProcessType.Survey => completionRole == CompletionRole.Employee, // Only employee in surveys
            _ => throw new ArgumentOutOfRangeException(nameof(processType), processType, "Unknown process type")
        };
    }

    /// <summary>
    /// Gets the display name for this process type (English only).
    /// For translated display names in Razor components, use GetDisplayNameKey() with @T().
    /// </summary>
    public static string GetDisplayName(this QuestionnaireProcessType processType)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => "Performance Review",
            QuestionnaireProcessType.Survey => "Survey",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets the translation key for the display name of this process type.
    /// Use with @T() in Razor components to get the translated display name.
    /// </summary>
    public static string GetDisplayNameKey(this QuestionnaireProcessType processType)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => "process-types.performance-review",
            QuestionnaireProcessType.Survey => "process-types.survey",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Gets the description for this process type (English only).
    /// For translated descriptions in Razor components, use GetDescriptionKey() with @T().
    /// </summary>
    public static string GetDescription(this QuestionnaireProcessType processType)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => "Performance review with manager participation and goal tracking",
            QuestionnaireProcessType.Survey => "Simple survey without manager involvement",
            _ => "Unknown process type"
        };
    }

    /// <summary>
    /// Gets the translation key for the description of this process type.
    /// Use with @T() in Razor components to get the translated description.
    /// </summary>
    public static string GetDescriptionKey(this QuestionnaireProcessType processType)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => "process-types.performance-review-description",
            QuestionnaireProcessType.Survey => "process-types.survey-description",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Gets the icon for this process type.
    /// </summary>
    public static string GetIcon(this QuestionnaireProcessType processType)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => "assessment",
            QuestionnaireProcessType.Survey => "poll",
            _ => "help"
        };
    }

    /// <summary>
    /// Gets the badge CSS class for this process type.
    /// </summary>
    public static string GetBadgeClass(this QuestionnaireProcessType processType)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => "badge bg-primary",
            QuestionnaireProcessType.Survey => "badge bg-info",
            _ => "badge bg-secondary"
        };
    }

    /// <summary>
    /// Gets the Radzen BadgeStyle for this process type.
    /// </summary>
    public static Radzen.BadgeStyle GetBadgeStyle(this QuestionnaireProcessType processType)
    {
        return processType switch
        {
            QuestionnaireProcessType.PerformanceReview => Radzen.BadgeStyle.Primary,
            QuestionnaireProcessType.Survey => Radzen.BadgeStyle.Info,
            _ => Radzen.BadgeStyle.Secondary
        };
    }

    /// <summary>
    /// Gets all available process types for UI selection.
    /// </summary>
    public static List<QuestionnaireProcessType> GetAllProcessTypes()
    {
        return new List<QuestionnaireProcessType>
        {
            QuestionnaireProcessType.PerformanceReview,
            QuestionnaireProcessType.Survey
        };
    }

    /// <summary>
    /// Gets a user-friendly error message when a question type is not allowed (English only).
    /// For translated messages in Razor components, use GetQuestionTypeRestrictionMessageKey() with @T().
    /// </summary>
    public static string GetQuestionTypeRestrictionMessage(this QuestionnaireProcessType processType, QuestionType questionType)
    {
        if (processType == QuestionnaireProcessType.Survey && questionType == QuestionType.Goal)
        {
            return "Goal questions are not allowed in surveys. Use Performance Review process type for goal tracking.";
        }
        if (processType == QuestionnaireProcessType.Survey && questionType == QuestionType.EmployeeFeedback)
        {
            return "Employee feedback questions are not allowed in surveys. Use Performance Review process type for external feedback collection.";
        }

        return "This question type is not allowed for the selected process type.";
    }

    /// <summary>
    /// Gets the translation key for the error message when a question type is not allowed.
    /// Use with @T() in Razor components to get the translated error message.
    /// </summary>
    public static string GetQuestionTypeRestrictionMessageKey(this QuestionnaireProcessType processType, QuestionType questionType)
    {
        if (processType == QuestionnaireProcessType.Survey && questionType == QuestionType.Goal)
        {
            return "validation.goals-not-allowed-in-surveys";
        }
        if (processType == QuestionnaireProcessType.Survey && questionType == QuestionType.EmployeeFeedback)
        {
            return "validation.employee-feedback-not-allowed-in-surveys";
        }

        return "validation.question-type-not-allowed";
    }

    /// <summary>
    /// Gets a user-friendly error message when a completion role is not allowed (English only).
    /// For translated messages in Razor components, use GetCompletionRoleRestrictionMessageKey() with @T().
    /// </summary>
    public static string GetCompletionRoleRestrictionMessage(this QuestionnaireProcessType processType, CompletionRole completionRole)
    {
        if (processType == QuestionnaireProcessType.Survey && completionRole != CompletionRole.Employee)
        {
            return "Surveys can only have Employee completion role. Manager and Both roles require Performance Review process type.";
        }

        return "This completion role is not allowed for the selected process type.";
    }

    /// <summary>
    /// Gets the translation key for the error message when a completion role is not allowed.
    /// Use with @T() in Razor components to get the translated error message.
    /// </summary>
    public static string GetCompletionRoleRestrictionMessageKey(this QuestionnaireProcessType processType, CompletionRole completionRole)
    {
        if (processType == QuestionnaireProcessType.Survey && completionRole != CompletionRole.Employee)
        {
            return "validation.manager-roles-not-allowed-in-surveys";
        }

        return "validation.completion-role-not-allowed";
    }
}
