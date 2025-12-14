namespace ti8m.BeachBreak.CommandApi.DTOs;

/// <summary>
/// DTO representing an evaluation rating with score and comment.
/// Provides validation and type safety for assessment responses.
/// </summary>
public class EvaluationRatingDto
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
