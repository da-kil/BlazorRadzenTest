namespace ti8m.BeachBreak.Client.Models.DTOs;

/// <summary>
/// Simple DTO for evaluation ratings.
/// </summary>
public class EvaluationRatingDto
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
