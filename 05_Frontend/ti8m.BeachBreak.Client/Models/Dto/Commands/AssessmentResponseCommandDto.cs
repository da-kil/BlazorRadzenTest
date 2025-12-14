namespace ti8m.BeachBreak.Client.Models.Dto.Commands;

/// <summary>
/// Command DTO for assessment question responses.
/// Provides type-safe evaluation rating access.
/// </summary>
public class AssessmentResponseCommandDto
{
    public Dictionary<string, EvaluationRatingCommandDto> Evaluations { get; set; } = new();
}