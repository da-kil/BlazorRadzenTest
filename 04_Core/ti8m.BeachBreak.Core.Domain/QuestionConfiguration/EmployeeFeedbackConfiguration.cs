namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

/// <summary>
/// Configuration for employee feedback questions that support configurable evaluation criteria
/// and multiple feedback source types (Customer, Peer, Project Colleague).
/// Leverages existing EvaluationItem and TextSectionDefinition patterns for UI consistency.
/// </summary>
public sealed class EmployeeFeedbackConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.EmployeeFeedback;

    /// <summary>
    /// Available evaluation criteria that can be selected for this feedback entry.
    /// Uses existing EvaluationItem pattern for UI consistency with assessments.
    /// </summary>
    public List<EvaluationItem> AvailableCriteria { get; set; } = new();

    /// <summary>
    /// Pre-selected evaluation criteria for this feedback template.
    /// Empty list allows dynamic criteria selection during data entry.
    /// </summary>
    public List<string> SelectedCriteriaKeys { get; set; } = new();

    /// <summary>
    /// Available text sections for unstructured feedback.
    /// Uses existing TextSectionDefinition pattern for UI consistency.
    /// </summary>
    public List<TextSectionDefinition> TextSections { get; set; } = new();

    /// <summary>
    /// Rating scale for evaluation criteria (e.g., 1-10).
    /// Defaults to 10-point scale for maximum granularity.
    /// </summary>
    public int RatingScale { get; set; } = 10;

    /// <summary>
    /// Key for the low end of the rating scale.
    /// Frontend uses @T("rating-scale.poor") for localized display.
    /// </summary>
    public string ScaleLowLabel { get; set; } = "";

    /// <summary>
    /// Key for the high end of the rating scale.
    /// Frontend uses @T("rating-scale.excellent") for localized display.
    /// </summary>
    public string ScaleHighLabel { get; set; } = "";

    /// <summary>
    /// Supported feedback source types for this configuration.
    /// Can restrict which source types are allowed.
    /// </summary>
    public List<int> AllowedSourceTypes { get; set; } = new() { 0, 1, 2 }; // Customer, Peer, ProjectColleague

    /// <summary>
    /// Whether project context is required for this feedback configuration.
    /// Automatically enforced for ProjectColleague feedback.
    /// </summary>
    public bool RequireProjectContext { get; set; } = false;

    /// <summary>
    /// Whether provider role information is required.
    /// </summary>
    public bool RequireProviderRole { get; set; } = false;

    /// <summary>
    /// Creates a default configuration for customer feedback with standard 7 criteria.
    /// </summary>
    public static EmployeeFeedbackConfiguration CreateCustomerFeedbackDefault()
    {
        return new EmployeeFeedbackConfiguration
        {
            AvailableCriteria = CreateDefaultCriteria(),
            SelectedCriteriaKeys = CreateDefaultCriteria().Select(c => c.Key).ToList(),
            TextSections = CreateDefaultTextSections(),
            RatingScale = 10,
            ScaleLowLabel = "",
            ScaleHighLabel = "",
            AllowedSourceTypes = new List<int> { 0 }, // Customer only
            RequireProjectContext = false,
            RequireProviderRole = false
        };
    }

    /// <summary>
    /// Creates a default configuration for peer feedback.
    /// </summary>
    public static EmployeeFeedbackConfiguration CreatePeerFeedbackDefault()
    {
        return new EmployeeFeedbackConfiguration
        {
            AvailableCriteria = CreateDefaultCriteria(),
            TextSections = CreateDefaultTextSections(),
            RatingScale = 10,
            ScaleLowLabel = "",
            ScaleHighLabel = "",
            AllowedSourceTypes = new List<int> { 1 }, // Peer only
            RequireProjectContext = false,
            RequireProviderRole = true
        };
    }

    /// <summary>
    /// Creates a default configuration for project colleague feedback.
    /// </summary>
    public static EmployeeFeedbackConfiguration CreateProjectColleagueFeedbackDefault()
    {
        return new EmployeeFeedbackConfiguration
        {
            AvailableCriteria = CreateDefaultCriteria(),
            TextSections = CreateDefaultTextSections(),
            RatingScale = 10,
            ScaleLowLabel = "",
            ScaleHighLabel = "",
            AllowedSourceTypes = new List<int> { 2 }, // Project Colleague only
            RequireProjectContext = true,
            RequireProviderRole = true
        };
    }

    /// <summary>
    /// Creates the default 7 evaluation criteria for employee feedback.
    /// </summary>
    private static List<EvaluationItem> CreateDefaultCriteria()
    {
        return new List<EvaluationItem>
        {
            new("overall_satisfaction", "Overall satisfaction", "Gesamtzufriedenheit", "", "", false, 0),
            new("leadership_behavior", "Leadership behavior", "Führungsverhalten", "", "", false, 1),
            new("technical_methodological_skills", "Technical and methodological skills", "Technische und methodische Fähigkeiten", "", "", false, 2),
            new("commitment", "Commitment", "Engagement", "", "", false, 3),
            new("reliability", "Reliability", "Zuverlässigkeit", "", "", false, 4),
            new("teamwork", "Teamwork", "Teamarbeit", "", "", false, 5),
            new("quality_of_work", "Quality of work and diligence", "Arbeitsqualität und Sorgfalt", "", "", false, 6)
        };
    }

    /// <summary>
    /// Creates the default 3 text sections for unstructured feedback.
    /// </summary>
    private static List<TextSectionDefinition> CreateDefaultTextSections()
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
}