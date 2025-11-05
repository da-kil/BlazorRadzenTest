namespace ti8m.BeachBreak.Client.Models.CommandDTOs;

/// <summary>
/// Command DTO for assessment question responses.
/// Provides type-safe competency rating access.
/// </summary>
public class AssessmentResponseCommandDto
{
    public Dictionary<string, CompetencyRatingCommandDto> Competencies { get; set; } = new();
}