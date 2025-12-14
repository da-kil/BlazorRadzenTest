using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service to handle questionnaire and question validation logic.
/// Centralizes validation rules and error message generation.
/// </summary>
public class QuestionnaireValidationService
{
    private readonly QuestionConfigurationService configurationService;

    public QuestionnaireValidationService(QuestionConfigurationService configurationService)
    {
        this.configurationService = configurationService;
    }

    /// <summary>
    /// Validates a questionnaire template and returns list of validation errors
    /// Matches the existing validation logic from QuestionnaireBuilder.razor
    /// </summary>
    public List<string> ValidateQuestionnaire(QuestionnaireTemplate template)
    {
        var validationErrors = new List<string>();

        // Validate template name - English name is required
        if (string.IsNullOrWhiteSpace(template.NameEnglish))
        {
            validationErrors.Add("English template name is required");
        }

        // Validate category - CategoryId is required
        if (template.CategoryId == Guid.Empty)
        {
            validationErrors.Add("Category is required");
        }

        // Validate workflow configuration
        if (!template.RequiresManagerReview)
        {
            // When manager review is not required, all sections must be employee-only
            var nonEmployeeSections = template.Sections
                .Where(s => s.CompletionRole != CompletionRole.Employee)
                .ToList();

            if (nonEmployeeSections.Any())
            {
                foreach (var section in nonEmployeeSections)
                {
                    var sectionName = string.IsNullOrWhiteSpace(section.TitleEnglish)
                        ? $"Section {section.Order + 1}"
                        : section.TitleEnglish;
                    validationErrors.Add($"'{sectionName}' must be completed by Employee only when manager review is not required");
                }
            }
        }
        else
        {
            // When manager review is required, at least one section must involve the manager
            var managerSections = template.Sections
                .Where(s => s.CompletionRole == CompletionRole.Manager || s.CompletionRole == CompletionRole.Both)
                .ToList();

            if (!managerSections.Any())
            {
                validationErrors.Add("When manager review is required, at least one section must be completed by 'Manager' or 'Both'");
            }
        }

        // Validate sections
        foreach (var section in template.Sections)
        {
            var sectionName = string.IsNullOrWhiteSpace(section.TitleEnglish)
                ? $"Section {section.Order + 1}"
                : section.TitleEnglish;

            // Validate section title - English title is required
            if (string.IsNullOrWhiteSpace(section.TitleEnglish))
            {
                validationErrors.Add($"'{sectionName}' requires an English title");
            }

            // Validate section content based on type
            if (section.Type == QuestionType.Assessment)
            {
                if (section.Configuration is not AssessmentConfiguration assessmentConfig)
                {
                    validationErrors.Add($"'{sectionName}' has Assessment type but invalid configuration");
                    continue;
                }

                if (assessmentConfig.Evaluations.Count == 0)
                {
                    validationErrors.Add($"'{sectionName}' must have at least one evaluation");
                }
                else
                {
                    for (int i = 0; i < assessmentConfig.Evaluations.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(assessmentConfig.Evaluations[i].TitleEnglish))
                        {
                            validationErrors.Add($"Evaluation {i + 1} in '{sectionName}' requires an English title");
                        }
                    }
                }
            }
            else if (section.Type == QuestionType.Goal)
            {
                if (section.Configuration is not GoalConfiguration)
                {
                    validationErrors.Add($"'{sectionName}' has Goal type but invalid configuration");
                }
                // Goal questions don't require template items - validated by configuration type only
            }
            else if (section.Type == QuestionType.TextQuestion)
            {
                if (section.Configuration is not TextQuestionConfiguration textConfig)
                {
                    validationErrors.Add($"'{sectionName}' has TextQuestion type but invalid configuration");
                    continue;
                }

                if (textConfig.TextSections.Count == 0)
                {
                    validationErrors.Add($"'{sectionName}' must have at least one text section");
                }
                else
                {
                    for (int i = 0; i < textConfig.TextSections.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(textConfig.TextSections[i].TitleEnglish) &&
                            string.IsNullOrWhiteSpace(textConfig.TextSections[i].TitleGerman))
                        {
                            validationErrors.Add($"Text section {i + 1} in '{sectionName}' requires a title (in English or German)");
                        }
                    }
                }
            }
        }

        return validationErrors;
    }

    /// <summary>
    /// Quick validation check - returns true if template is valid
    /// </summary>
    public bool IsValid(QuestionnaireTemplate template)
    {
        return !ValidateQuestionnaire(template).Any();
    }

    /// <summary>
    /// Gets a user-friendly validation summary message
    /// </summary>
    public string GetValidationSummary(QuestionnaireTemplate template)
    {
        var errors = ValidateQuestionnaire(template);
        if (!errors.Any())
        {
            return "Validation successful";
        }

        return $"Found {errors.Count} validation error(s):\n" + string.Join("\n", errors);
    }
}
