namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

/// <summary>
/// Service to manage configurable feedback templates and evaluation criteria.
/// Provides default templates and supports custom criteria selection.
/// </summary>
public static class FeedbackTemplateManager
{
    /// <summary>
    /// Gets the default template for a specific feedback source type.
    /// </summary>
    /// <param name="sourceType">The feedback source type (0=Customer, 1=Peer, 2=ProjectColleague)</param>
    /// <returns>Default configuration for the specified source type</returns>
    public static EmployeeFeedbackConfiguration GetDefaultTemplate(int sourceType)
    {
        return sourceType switch
        {
            0 => EmployeeFeedbackConfiguration.CreateCustomerFeedbackDefault(),
            1 => EmployeeFeedbackConfiguration.CreatePeerFeedbackDefault(),
            2 => EmployeeFeedbackConfiguration.CreateProjectColleagueFeedbackDefault(),
            _ => throw new ArgumentException($"Unknown feedback source type: {sourceType}", nameof(sourceType))
        };
    }

    /// <summary>
    /// Gets all available evaluation criteria that can be used in feedback forms.
    /// This includes the standard 7 criteria plus any additional custom criteria.
    /// </summary>
    public static List<EvaluationItem> GetAllAvailableCriteria()
    {
        var criteria = new List<EvaluationItem>
        {
            // Standard 7 criteria
            new("overall_satisfaction", "Overall satisfaction", "Gesamtzufriedenheit", "", "", false, 0),
            new("leadership_behavior", "Leadership behavior", "Führungsverhalten", "", "", false, 1),
            new("technical_methodological_skills", "Technical and methodological skills", "Technische und methodische Fähigkeiten", "", "", false, 2),
            new("commitment", "Commitment", "Engagement", "", "", false, 3),
            new("reliability", "Reliability", "Zuverlässigkeit", "", "", false, 4),
            new("teamwork", "Teamwork", "Teamarbeit", "", "", false, 5),
            new("quality_of_work", "Quality of work and diligence", "Arbeitsqualität und Sorgfalt", "", "", false, 6),

            // Additional criteria that can be used
            new("communication_skills", "Communication skills", "Kommunikationsfähigkeiten", "", "", false, 7),
            new("problem_solving", "Problem solving", "Problemlösungsfähigkeit", "", "", false, 8),
            new("creativity_innovation", "Creativity and innovation", "Kreativität und Innovation", "", "", false, 9),
            new("adaptability", "Adaptability", "Anpassungsfähigkeit", "", "", false, 10),
            new("time_management", "Time management", "Zeitmanagement", "", "", false, 11),
            new("customer_focus", "Customer focus", "Kundenorientierung", "", "", false, 12),
            new("initiative", "Initiative", "Initiative", "", "", false, 13),
            new("mentoring_coaching", "Mentoring and coaching", "Mentoring und Coaching", "", "", false, 14),
            new("project_management", "Project management", "Projektmanagement", "", "", false, 15)
        };

        return criteria;
    }

    /// <summary>
    /// Gets the standard text sections for unstructured feedback.
    /// </summary>
    public static List<TextSectionDefinition> GetStandardTextSections()
    {
        return new List<TextSectionDefinition>
        {
            new()
            {
                Key = "positive_impressions",
                TitleEnglish = "Positive impressions",
                TitleGerman = "Positive Eindrücke",
                PlaceholderEnglish = "What positive aspects did you observe?",
                PlaceholderGerman = "Welche positiven Aspekte haben Sie beobachtet?",
                IsRequired = false,
                Order = 0
            },
            new()
            {
                Key = "potential_for_improvement",
                TitleEnglish = "Potential for improvement",
                TitleGerman = "Verbesserungspotential",
                PlaceholderEnglish = "What areas could be improved?",
                PlaceholderGerman = "Welche Bereiche könnten verbessert werden?",
                IsRequired = false,
                Order = 1
            },
            new()
            {
                Key = "general_comments",
                TitleEnglish = "General comments",
                TitleGerman = "Allgemeine Kommentare",
                PlaceholderEnglish = "Any additional comments or observations?",
                PlaceholderGerman = "Weitere Kommentare oder Beobachtungen?",
                IsRequired = false,
                Order = 2
            }
        };
    }

    /// <summary>
    /// Creates a custom feedback configuration with selected criteria and text sections.
    /// </summary>
    /// <param name="selectedCriteriaKeys">Keys of evaluation criteria to include</param>
    /// <param name="textSectionKeys">Keys of text sections to include</param>
    /// <param name="sourceType">Feedback source type</param>
    /// <param name="ratingScale">Rating scale (default: 10)</param>
    /// <returns>Custom feedback configuration</returns>
    public static EmployeeFeedbackConfiguration CreateCustomTemplate(
        List<string> selectedCriteriaKeys,
        List<string>? textSectionKeys = null,
        int sourceType = 0,
        int ratingScale = 10)
    {
        var allCriteria = GetAllAvailableCriteria();
        var allTextSections = GetStandardTextSections();

        var selectedCriteria = allCriteria
            .Where(c => selectedCriteriaKeys.Contains(c.Key))
            .ToList();

        var selectedTextSections = textSectionKeys != null
            ? allTextSections.Where(t => textSectionKeys.Contains(t.Key)).ToList()
            : allTextSections;

        return new EmployeeFeedbackConfiguration
        {
            AvailableCriteria = selectedCriteria,
            SelectedCriteriaKeys = selectedCriteriaKeys,
            TextSections = selectedTextSections,
            RatingScale = ratingScale,
            ScaleLowLabel = "",
            ScaleHighLabel = "",
            AllowedSourceTypes = new List<int> { sourceType },
            RequireProjectContext = sourceType == 2, // ProjectColleague
            RequireProviderRole = sourceType != 0 // Not Customer
        };
    }

    /// <summary>
    /// Validates that a feedback configuration is valid for the specified source type.
    /// </summary>
    /// <param name="configuration">Configuration to validate</param>
    /// <param name="sourceType">Target source type</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool ValidateConfigurationForSourceType(EmployeeFeedbackConfiguration configuration, int sourceType)
    {
        // Check if source type is allowed
        if (!configuration.AllowedSourceTypes.Contains(sourceType))
            return false;

        // Check project context requirement for ProjectColleague
        if (sourceType == 2 && !configuration.RequireProjectContext)
            return false;

        // Ensure at least one evaluation criteria or text section is available
        if (!configuration.AvailableCriteria.Any() && !configuration.TextSections.Any())
            return false;

        // Rating scale should be between 2 and 10
        if (configuration.RatingScale < 2 || configuration.RatingScale > 10)
            return false;

        return true;
    }

    /// <summary>
    /// Gets the source type key for localized display.
    /// Frontend uses @T("source-types.{type}.name") for localized display.
    /// </summary>
    /// <param name="sourceType">Source type (0=Customer, 1=Peer, 2=ProjectColleague)</param>
    /// <returns>Source type key for translation lookup</returns>
    public static string GetSourceTypeKey(int sourceType)
    {
        return sourceType switch
        {
            0 => "customer",
            1 => "peer",
            2 => "project-colleague",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Gets a list of source type IDs.
    /// Frontend uses @T("source-types.{key}.name") for localized display names.
    /// </summary>
    public static List<(int Value, string Key)> GetSourceTypeOptions()
    {
        return new List<(int, string)>
        {
            (0, "customer"),
            (1, "peer"),
            (2, "project-colleague")
        };
    }
}