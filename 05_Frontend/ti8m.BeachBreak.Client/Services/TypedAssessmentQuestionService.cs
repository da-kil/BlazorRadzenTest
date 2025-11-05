using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.DTOs;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Type-safe service for assessment question operations.
/// Eliminates magic string keys and fixes the "Rating_" vs "competency_" format bug.
/// Standardizes on the OptimizedAssessmentQuestion format.
/// </summary>
public class TypedAssessmentQuestionService
{
    /// <summary>
    /// Creates an empty assessment response.
    /// </summary>
    public static AssessmentResponseDto CreateAssessmentResponse() => new();

    /// <summary>
    /// Sets a competency rating in a type-safe way.
    /// NO MORE MAGIC STRINGS!
    /// </summary>
    public static void SetCompetencyRating(AssessmentResponseDto response, string competencyKey, int rating, string comment = "")
    {
        response.SetCompetency(competencyKey, rating, comment);
    }

    /// <summary>
    /// Gets a competency rating safely.
    /// </summary>
    public static CompetencyRatingDto? GetCompetencyRating(AssessmentResponseDto? response, string competencyKey)
    {
        return response?.GetCompetency(competencyKey);
    }

    /// <summary>
    /// Validates that an assessment response is complete.
    /// Eliminates scattered validation logic.
    /// </summary>
    public static bool IsAssessmentComplete(AssessmentResponseDto? response, QuestionItem question)
    {
        if (response == null) return false;

        var configJson = System.Text.Json.JsonSerializer.Serialize(question.Configuration);
        var competencies = GetCompetenciesFromConfiguration(configJson);
        var requiredCompetencies = competencies.Where(c => c.IsRequired).Select(c => c.Key);

        return response.IsComplete(requiredCompetencies);
    }

    /// <summary>
    /// Gets competencies configuration from question without duplicate parsing.
    /// Centralizes configuration parsing logic.
    /// </summary>
    public static List<CompetencyItem> GetCompetenciesFromConfiguration(string? configuration)
    {
        if (string.IsNullOrEmpty(configuration))
            return [];

        try
        {
            var config = System.Text.Json.JsonSerializer.Deserialize<AssessmentQuestionConfiguration>(configuration);
            return config?.Competencies?.OrderBy(c => c.Order).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Gets the rating scale from question configuration.
    /// </summary>
    public static RatingScale GetRatingScale(string? configuration)
    {
        if (string.IsNullOrEmpty(configuration))
            return RatingScale.Default;

        try
        {
            var config = System.Text.Json.JsonSerializer.Deserialize<AssessmentQuestionConfiguration>(configuration);
            return config?.RatingScale ?? RatingScale.Default;
        }
        catch
        {
            return RatingScale.Default;
        }
    }
}

/// <summary>
/// Assessment question configuration model.
/// </summary>
public class AssessmentQuestionConfiguration
{
    public List<CompetencyItem>? Competencies { get; set; }
    public RatingScale? RatingScale { get; set; }
}

/// <summary>
/// Competency item configuration.
/// </summary>
public class CompetencyItem
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
}

/// <summary>
/// Rating scale configuration.
/// </summary>
public class RatingScale
{
    public int MinValue { get; set; } = 0;
    public int MaxValue { get; set; } = 4;
    public Dictionary<int, string> Descriptions { get; set; } = new();

    public static RatingScale Default => new()
    {
        MinValue = 0,
        MaxValue = 4,
        Descriptions = new Dictionary<int, string>
        {
            { 0, "Not Demonstrated" },
            { 1, "Basic" },
            { 2, "Developing" },
            { 3, "Proficient" },
            { 4, "Advanced" }
        }
    };
}