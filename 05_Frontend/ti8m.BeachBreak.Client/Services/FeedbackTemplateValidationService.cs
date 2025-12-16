using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Provides validation for feedback templates before creation or publishing.
/// Ensures templates meet all business rules and data quality requirements.
/// </summary>
public class FeedbackTemplateValidationService
{
    /// <summary>
    /// Validates a feedback template and returns a list of validation errors.
    /// </summary>
    /// <param name="template">The template to validate</param>
    /// <returns>List of validation error messages (empty if valid)</returns>
    public List<string> ValidateTemplate(FeedbackTemplate template)
    {
        var errors = new List<string>();

        // Basic required fields
        if (string.IsNullOrWhiteSpace(template.NameEnglish))
            errors.Add("Template name (English) is required");

        // Content validation - must have at least one way to provide feedback
        if (template.Criteria.Count == 0 && template.TextSections.Count == 0)
            errors.Add("Template must have at least one evaluation criterion or text section");

        // Rating scale validation
        if (template.RatingScale < 2 || template.RatingScale > 10)
            errors.Add("Rating scale must be between 2 and 10");

        if (string.IsNullOrWhiteSpace(template.ScaleLowLabel) || string.IsNullOrWhiteSpace(template.ScaleHighLabel))
            errors.Add("Rating scale labels are required");

        // Duplicate key validation - criteria
        var duplicateCriteriaKeys = template.Criteria
            .GroupBy(c => c.Key)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateCriteriaKeys.Any())
            errors.Add($"Duplicate criterion keys found: {string.Join(", ", duplicateCriteriaKeys)}");

        // Duplicate key validation - text sections
        var duplicateSectionKeys = template.TextSections
            .GroupBy(s => s.Key)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateSectionKeys.Any())
            errors.Add($"Duplicate text section keys found: {string.Join(", ", duplicateSectionKeys)}");

        // Source type validation
        if (template.AllowedSourceTypes == null || template.AllowedSourceTypes.Count == 0)
            errors.Add("Template must allow at least one feedback source type");

        return errors;
    }

    /// <summary>
    /// Checks if a template is valid (has no validation errors).
    /// </summary>
    /// <param name="template">The template to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsTemplateValid(FeedbackTemplate template)
    {
        return ValidateTemplate(template).Count == 0;
    }

    /// <summary>
    /// Gets a user-friendly validation summary message.
    /// </summary>
    /// <param name="template">The template to validate</param>
    /// <returns>Summary message describing validation status</returns>
    public string GetValidationSummary(FeedbackTemplate template)
    {
        var errors = ValidateTemplate(template);

        if (errors.Count == 0)
            return "Template is valid and ready to publish";

        return $"Template has {errors.Count} validation issue(s): {string.Join("; ", errors)}";
    }
}
