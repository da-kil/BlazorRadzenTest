namespace ti8m.BeachBreak.Client.Models.Dto.Commands;

/// <summary>
/// Command DTO for competency ratings with validation.
/// </summary>
public class CompetencyRatingCommandDto
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}