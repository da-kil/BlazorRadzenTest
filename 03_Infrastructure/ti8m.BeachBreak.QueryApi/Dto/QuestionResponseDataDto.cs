using System.Text.Json.Serialization;
using ti8m.BeachBreak.Application.Query.Models;

namespace ti8m.BeachBreak.QueryApi.Dto;

/// <summary>
/// Base class for different types of question response data.
/// This maintains type safety while being independent of domain value objects.
/// </summary>
[JsonDerivedType(typeof(AssessmentResponseDataDto), typeDiscriminator: 0)]
[JsonDerivedType(typeof(TextResponseDataDto), typeDiscriminator: 1)]
[JsonDerivedType(typeof(GoalResponseDataDto), typeDiscriminator: 2)]
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
/// Response data for assessment questions with evaluation ratings.
/// </summary>
public class AssessmentResponseDataDto : QuestionResponseDataDto
{
    public Dictionary<string, EvaluationRatingDto> Evaluations { get; set; } = new();
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
/// Evaluation rating data for assessments.
/// </summary>
public class EvaluationRatingDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

/// <summary>
/// Goal data for goal responses.
/// Must match frontend GoalDataDto structure exactly for compatibility.
/// </summary>
public class GoalDataDto
{
    public Guid GoalId { get; set; }
    public string ObjectiveDescription { get; set; } = string.Empty;
    public DateTime TimeframeFrom { get; set; }
    public DateTime TimeframeTo { get; set; }
    public string MeasurementMetric { get; set; } = string.Empty;
    public decimal WeightingPercentage { get; set; }
    public ApplicationRole AddedByRole { get; set; }
}

/// <summary>
/// Predecessor rating data for goal responses.
/// </summary>
public class PredecessorRatingDto
{
    public Guid SourceGoalId { get; set; }
    public int DegreeOfAchievement { get; set; }
    public string Justification { get; set; } = string.Empty;
    public ApplicationRole RatedByRole { get; set; }
    public string OriginalObjective { get; set; } = string.Empty;
    public ApplicationRole OriginalAddedByRole { get; set; }
}

