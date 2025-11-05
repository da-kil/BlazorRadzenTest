namespace ti8m.BeachBreak.CommandApi.DTOs;

/// <summary>
/// DTO representing a competency rating with score and comment.
/// Provides validation and type safety for assessment responses.
/// </summary>
public class CompetencyRatingDto
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}