using System.Text.Json.Serialization;
using ti8m.BeachBreak.Client.Models.Dto.Queries;

namespace ti8m.BeachBreak.Client.Models.DTOs;

/// <summary>
/// Base class for different types of question response data.
/// This maintains type safety while matching the API structure.
/// Uses custom QuestionResponseDataDtoJsonConverter for polymorphic deserialization.
/// </summary>
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
/// Response data for goal questions - reuses existing DTOs.
/// </summary>
public class GoalResponseDataDto : QuestionResponseDataDto
{
    public List<GoalDataDto> Goals { get; set; } = new();
    public List<PredecessorRatingDto> PredecessorRatings { get; set; } = new();
    public Guid? PredecessorAssignmentId { get; set; }
}