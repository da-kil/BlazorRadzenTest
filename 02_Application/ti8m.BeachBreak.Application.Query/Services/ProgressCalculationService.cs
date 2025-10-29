using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;
using QuestionnaireTemplate = ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries.QuestionnaireTemplate;
using QuestionItem = ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries.QuestionItem;
using QuestionType = ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.QuestionType;

namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Calculates role-based progress for questionnaire responses.
/// Only counts required questions and validates answers appropriately per question type.
/// </summary>
public class ProgressCalculationService : IProgressCalculationService
{
    private readonly ILogger<ProgressCalculationService> logger;

    public ProgressCalculationService(ILogger<ProgressCalculationService> logger)
    {
        this.logger = logger;
    }

    public ProgressCalculation Calculate(
        QuestionnaireTemplate template,
        Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, object>>> sectionResponses)
    {
        var result = new ProgressCalculation();

        if (template == null || template.Sections == null || !template.Sections.Any())
        {
            logger.LogWarning("Template is null or has no sections");
            return result; // All zeros
        }

        // Step 1: Count total REQUIRED questions per role from template
        var (employeeTotalQuestions, managerTotalQuestions) = CountTotalRequiredQuestions(template);

        result.EmployeeTotalQuestions = employeeTotalQuestions;
        result.ManagerTotalQuestions = managerTotalQuestions;

        // Step 2: Count answered REQUIRED questions per role from responses
        if (sectionResponses != null && sectionResponses.Any())
        {
            var (employeeAnsweredCount, managerAnsweredCount) =
                CountAnsweredRequiredQuestions(template, sectionResponses);

            result.EmployeeAnsweredQuestions = employeeAnsweredCount;
            result.ManagerAnsweredQuestions = managerAnsweredCount;
        }

        // Step 3: Calculate percentages
        if (employeeTotalQuestions > 0)
        {
            result.EmployeeProgress = (double)result.EmployeeAnsweredQuestions / employeeTotalQuestions * 100.0;
        }

        if (managerTotalQuestions > 0)
        {
            result.ManagerProgress = (double)result.ManagerAnsweredQuestions / managerTotalQuestions * 100.0;
        }

        // Step 4: Calculate weighted overall progress
        // "Both" sections count 2× (once for employee, once for manager)
        result.OverallProgress = CalculateOverallProgress(
            employeeTotalQuestions,
            result.EmployeeAnsweredQuestions,
            managerTotalQuestions,
            result.ManagerAnsweredQuestions);

        return result;
    }

    /// <summary>
    /// Counts total required questions per role from the template.
    /// Sections with CompletionRole.Both count for BOTH roles.
    /// </summary>
    private (int employeeTotal, int managerTotal) CountTotalRequiredQuestions(QuestionnaireTemplate template)
    {
        var employeeTotal = 0;
        var managerTotal = 0;

        foreach (var section in template.Sections)
        {
            // Only count required questions
            var requiredQuestionCount = section.Questions.Count(q => q.IsRequired);

            // Query model uses string for CompletionRole
            var roleStr = section.CompletionRole;

            if (roleStr == "Employee")
            {
                employeeTotal += requiredQuestionCount;
            }
            else if (roleStr == "Manager")
            {
                managerTotal += requiredQuestionCount;
            }
            else if (roleStr == "Both")
            {
                // Both roles must answer - count for each
                employeeTotal += requiredQuestionCount;
                managerTotal += requiredQuestionCount;
            }
        }

        return (employeeTotal, managerTotal);
    }

    /// <summary>
    /// Counts answered required questions per role from the response data.
    /// </summary>
    private (int employeeAnswered, int managerAnswered) CountAnsweredRequiredQuestions(
        QuestionnaireTemplate template,
        Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, object>>> sectionResponses)
    {
        var employeeAnswered = 0;
        var managerAnswered = 0;

        foreach (var section in template.Sections)
        {
            // Skip if no responses for this section
            if (!sectionResponses.TryGetValue(section.Id, out var roleResponses))
            {
                continue;
            }

            // Get only required questions
            var requiredQuestions = section.Questions.Where(q => q.IsRequired).ToList();

            // Parse completion role (query model uses string, domain uses enum)
            var completionRoleStr = section.CompletionRole;

            // Count employee answers for this section
            if (completionRoleStr == "Employee" || completionRoleStr == "Both")
            {
                if (roleResponses.TryGetValue(CompletionRole.Employee, out var employeeAnswers))
                {
                    employeeAnswered += CountValidAnswers(employeeAnswers, requiredQuestions);
                }
            }

            // Count manager answers for this section
            if (completionRoleStr == "Manager" || completionRoleStr == "Both")
            {
                if (roleResponses.TryGetValue(CompletionRole.Manager, out var managerAnswers))
                {
                    managerAnswered += CountValidAnswers(managerAnswers, requiredQuestions);
                }
            }
        }

        return (employeeAnswered, managerAnswered);
    }

    /// <summary>
    /// Counts valid answers for a list of required questions.
    /// Validates answers based on question type requirements.
    /// </summary>
    private int CountValidAnswers(
        Dictionary<Guid, object> answers,
        List<QuestionItem> requiredQuestions)
    {
        var count = 0;

        foreach (var question in requiredQuestions)
        {
            if (answers.TryGetValue(question.Id, out var answer))
            {
                // Convert Query QuestionType (enum) to Domain QuestionType (enum)
                // Both enums have the same values, we just need to parse the string representation
                var domainType = Enum.Parse<QuestionType>(question.Type.ToString());
                if (IsValidAnswer(answer, domainType))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Validates if an answer is valid based on the question type.
    ///
    /// Rules:
    /// - Assessment: Must have a rating (1-4). Comment text is optional.
    /// - TextQuestion: Must have non-empty text if required.
    /// - GoalAchievement: Must have non-null value.
    /// </summary>
    private bool IsValidAnswer(object answer, QuestionType type)
    {
        if (answer == null)
        {
            return false;
        }

        try
        {
            switch (type)
            {
                case QuestionType.Assessment:
                    // Assessment questions need a rating value
                    // The answer structure is typically a dictionary with Rating and Comment
                    return ValidateAssessmentAnswer(answer);

                case QuestionType.TextQuestion:
                    // Text questions must have non-empty text
                    var text = answer.ToString();
                    return !string.IsNullOrWhiteSpace(text);

                case QuestionType.Goal:
                    // Goal must have a value
                    return ValidateGoalAnswer(answer);

                default:
                    logger.LogWarning("Unknown question type: {Type}", type);
                    return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error validating answer for question type {Type}", type);
            return false;
        }
    }

    /// <summary>
    /// Validates Assessment answer structure.
    /// Must have a valid rating (1-4). Comment is optional.
    /// </summary>
    private bool ValidateAssessmentAnswer(object answer)
    {
        // Assessment answers are typically stored as JSON objects with Rating and Comment properties
        // When deserialized, they come as Dictionary<string, object> or JsonElement

        if (answer is System.Text.Json.JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Object &&
                jsonElement.TryGetProperty("Rating", out var ratingProp))
            {
                if (ratingProp.TryGetInt32(out var rating))
                {
                    return rating >= 1 && rating <= 4;
                }
            }
            return false;
        }

        if (answer is Dictionary<string, object> dict)
        {
            if (dict.TryGetValue("Rating", out var ratingObj))
            {
                if (ratingObj is int rating)
                {
                    return rating >= 1 && rating <= 4;
                }
                if (int.TryParse(ratingObj?.ToString(), out var parsedRating))
                {
                    return parsedRating >= 1 && parsedRating <= 4;
                }
            }
            return false;
        }

        // Fallback: try to parse as integer directly (in case it's just the rating value)
        if (answer is int directRating)
        {
            return directRating >= 1 && directRating <= 4;
        }

        if (int.TryParse(answer.ToString(), out var numericRating))
        {
            return numericRating >= 1 && numericRating <= 4;
        }

        return false;
    }

    /// <summary>
    /// Validates Goal answer structure.
    /// Must have a non-null, non-empty value.
    /// </summary>
    private bool ValidateGoalAnswer(object answer)
    {
        // Goal achievement answers are typically complex objects
        // Basic validation: not null and not empty string
        if (answer is string text)
        {
            return !string.IsNullOrWhiteSpace(text);
        }

        // If it's a complex object (dict or JsonElement), consider it valid if not null
        if (answer is Dictionary<string, object> || answer is System.Text.Json.JsonElement)
        {
            return true;
        }

        return answer != null;
    }

    /// <summary>
    /// Calculates weighted overall progress.
    /// "Both" sections count 2× because both roles must complete them.
    /// </summary>
    private double CalculateOverallProgress(
        int employeeTotal,
        int employeeAnswered,
        int managerTotal,
        int managerAnswered)
    {
        var totalQuestions = employeeTotal + managerTotal;
        var totalAnswered = employeeAnswered + managerAnswered;

        if (totalQuestions == 0)
        {
            return 0.0;
        }

        // Weighted calculation: Both sections naturally count 2× because they're
        // counted in both employeeTotal and managerTotal
        return (double)totalAnswered / totalQuestions * 100.0;
    }
}
