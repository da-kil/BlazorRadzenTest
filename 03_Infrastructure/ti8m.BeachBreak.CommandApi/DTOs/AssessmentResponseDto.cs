namespace ti8m.BeachBreak.CommandApi.DTOs;

/// <summary>
/// DTO for assessment question responses with competency ratings.
/// Eliminates magic string keys for competency access.
/// </summary>
public class AssessmentResponseDto
{
    public Dictionary<string, CompetencyRatingDto> Competencies { get; set; } = new();
}