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

        // Validate sections and questions
        foreach (var section in template.Sections)
        {
            // Check if section has any questions
            if (section.Questions.Count == 0)
            {
                var sectionName = string.IsNullOrWhiteSpace(section.TitleEnglish) ? $"Section {section.Order + 1}" : section.TitleEnglish;
                validationErrors.Add($"'{sectionName}' must contain at least one question");
                continue;
            }

            // Validate each question in the section
            foreach (var question in section.Questions)
            {
                var sectionName = string.IsNullOrWhiteSpace(section.TitleEnglish) ? $"Section {section.Order + 1}" : section.TitleEnglish;
                var questionPos = $"Question {question.Order + 1}";

                // Validate question title - English title is required
                if (string.IsNullOrWhiteSpace(question.TitleEnglish))
                {
                    validationErrors.Add($"{questionPos} in '{sectionName}' requires an English title");
                }

                // Validate question content based on type
                if (question.Type == QuestionType.Assessment)
                {
                    var competencies = configurationService.GetEvaluations(question);
                    if (competencies.Count == 0)
                    {
                        validationErrors.Add($"{questionPos} in '{sectionName}' must have at least one competency");
                    }
                    else
                    {
                        for (int i = 0; i < competencies.Count; i++)
                        {
                            if (string.IsNullOrWhiteSpace(competencies[i].TitleEnglish))
                            {
                                validationErrors.Add($"Competency {i + 1} in {questionPos} ('{sectionName}') requires an English title");
                            }
                        }
                    }
                }
                else if (question.Type == QuestionType.Goal)
                {
                    // Goal questions don't require template items - items are added dynamically during in-progress states
                    // Only Title and Description are required, which are already validated as QuestionItem properties
                }
                else if (question.Type == QuestionType.TextQuestion)
                {
                    var textSections = configurationService.GetTextSections(question);
                    if (textSections.Count == 0)
                    {
                        validationErrors.Add($"{questionPos} in '{sectionName}' must have at least one text section");
                    }
                    else
                    {
                        for (int i = 0; i < textSections.Count; i++)
                        {
                            if (string.IsNullOrWhiteSpace(textSections[i].TitleEnglish) &&
                                string.IsNullOrWhiteSpace(textSections[i].TitleGerman))
                            {
                                validationErrors.Add($"Text section {i + 1} in {questionPos} ('{sectionName}') requires a title (in English or German)");
                            }
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
