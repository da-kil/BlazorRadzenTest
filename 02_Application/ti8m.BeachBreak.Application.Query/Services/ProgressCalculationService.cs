using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;
using QuestionnaireTemplate = ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries.QuestionnaireTemplate;

namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Calculates role-based progress for questionnaire responses.
/// Only counts required sections and validates answers appropriately per section type.
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
        Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>> sectionResponses)
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
    /// Counts total required sections per role from the template.
    /// Sections with CompletionRole.Both count for BOTH roles.
    /// NOTE: Includes instance-specific (custom) sections. For aggregate analysis across multiple
    /// questionnaire instances, filter out sections with IsInstanceSpecific=true before counting.
    /// </summary>
    private (int employeeTotal, int managerTotal) CountTotalRequiredQuestions(QuestionnaireTemplate template)
    {
        var employeeTotal = 0;
        var managerTotal = 0;

        foreach (var section in template.Sections)
        {
            // Only count if section itself is required
            if (!section.IsRequired)
                continue;

            // Query model uses string for CompletionRole
            var roleStr = section.CompletionRole;

            if (roleStr == "Employee")
            {
                employeeTotal++;
            }
            else if (roleStr == "Manager")
            {
                managerTotal++;
            }
            else if (roleStr == "Both")
            {
                // Both roles must answer - count for each
                employeeTotal++;
                managerTotal++;
            }
        }

        return (employeeTotal, managerTotal);
    }

    /// <summary>
    /// Counts answered required sections per role from the response data.
    /// </summary>
    private (int employeeAnswered, int managerAnswered) CountAnsweredRequiredQuestions(
        QuestionnaireTemplate template,
        Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>> sectionResponses)
    {
        var employeeAnswered = 0;
        var managerAnswered = 0;

        foreach (var section in template.Sections)
        {
            // Only count required sections (Section IS the question)
            if (!section.IsRequired)
                continue;

            // Skip if no responses for this section
            if (!sectionResponses.TryGetValue(section.Id, out var roleResponses))
            {
                continue;
            }

            // Parse completion role (query model uses string, domain uses enum)
            var completionRoleStr = section.CompletionRole;

            // Count employee answer for this section
            if (completionRoleStr == "Employee" || completionRoleStr == "Both")
            {
                if (roleResponses.TryGetValue(CompletionRole.Employee, out var employeeAnswer))
                {
                    // Parse section type (query model string to domain enum)
                    if (Enum.TryParse<QuestionType>(section.Type, out var sectionType))
                    {
                        if (IsValidAnswer(employeeAnswer, sectionType))
                        {
                            employeeAnswered++;
                        }
                    }
                }
            }

            // Count manager answer for this section
            if (completionRoleStr == "Manager" || completionRoleStr == "Both")
            {
                if (roleResponses.TryGetValue(CompletionRole.Manager, out var managerAnswer))
                {
                    if (Enum.TryParse<QuestionType>(section.Type, out var sectionType))
                    {
                        if (IsValidAnswer(managerAnswer, sectionType))
                        {
                            managerAnswered++;
                        }
                    }
                }
            }
        }

        return (employeeAnswered, managerAnswered);
    }

    /// <summary>
    /// Validates if an answer is valid based on the question type.
    ///
    /// Rules:
    /// - Assessment: Must have a rating (1-4). Comment text is optional.
    /// - TextQuestion: Must have non-empty text if required.
    /// - GoalAchievement: Must have non-null value.
    /// </summary>
    private bool IsValidAnswer(QuestionResponseValue answer, QuestionType type)
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
                    return answer is QuestionResponseValue.AssessmentResponse assessmentResponse &&
                           assessmentResponse.Evaluations.Any(c => c.Value.IsValidRating);

                case QuestionType.TextQuestion:
                    // Text questions must have non-empty text
                    return answer is QuestionResponseValue.TextResponse textResponse &&
                           textResponse.TextSections.Any(section => !string.IsNullOrWhiteSpace(section));

                case QuestionType.Goal:
                    // Goal must have valid goals
                    return answer is QuestionResponseValue.GoalResponse goalResponse &&
                           goalResponse.Goals.Any(g => g.IsValid);

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
