using System.Text.Json.Serialization;

namespace ti8m.BeachBreak.QueryApi.Dto;

/// <summary>
/// Base class for different types of question response data.
/// This maintains type safety while being independent of domain value objects.
/// </summary>
[JsonDerivedType(typeof(TextResponseDataDto), typeDiscriminator: "text")]
[JsonDerivedType(typeof(AssessmentResponseDataDto), typeDiscriminator: "assessment")]
[JsonDerivedType(typeof(GoalResponseDataDto), typeDiscriminator: "goal")]
public abstract class QuestionResponseDataDto
{
}

/// <summary>
/// Response data for text-based questions.
/// </summary>
public class TextResponseDataDto : QuestionResponseDataDto
{
    public List<string> TextSections { get; set; } = new();
}

/// <summary>
/// Response data for assessment questions with competency ratings.
/// </summary>
public class AssessmentResponseDataDto : QuestionResponseDataDto
{
    public Dictionary<string, CompetencyRatingDto> Competencies { get; set; } = new();
}

/// <summary>
/// Response data for goal questions.
/// </summary>
public class GoalResponseDataDto : QuestionResponseDataDto
{
    public List<GoalDataDto> Goals { get; set; } = new();
    public List<PredecessorRatingDto> PredecessorRatings { get; set; } = new();
    public Guid? PredecessorAssignmentId { get; set; }
}

/// <summary>
/// Competency rating data for assessments.
/// </summary>
public class CompetencyRatingDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

/// <summary>
/// Goal data for goal responses.
/// </summary>
public class GoalDataDto
{
    public string Description { get; set; } = string.Empty;
    public double AchievementPercentage { get; set; }
    public string? Justification { get; set; }
    public double Weight { get; set; }
}

/// <summary>
/// Predecessor rating data for goal responses.
/// </summary>
public class PredecessorRatingDto
{
    public string GoalDescription { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
}