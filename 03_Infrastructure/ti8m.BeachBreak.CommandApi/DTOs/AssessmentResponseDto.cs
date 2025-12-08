namespace ti8m.BeachBreak.CommandApi.DTOs;

/// <summary>
/// DTO for assessment question responses with evaluation ratings.
/// Eliminates magic string keys for evaluation access.
/// </summary>
public class AssessmentResponseDto
{
    public Dictionary<string, EvaluationRatingDto> Evaluations { get; set; } = new();
}