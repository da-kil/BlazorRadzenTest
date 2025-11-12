namespace ti8m.BeachBreak.Client.Models.DTOs;

/// <summary>
/// Simple DTO for competency ratings.
/// </summary>
public class CompetencyRatingDto
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}