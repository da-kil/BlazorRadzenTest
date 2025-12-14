namespace ti8m.BeachBreak.Client.Models.Dto.Commands;

/// <summary>
/// Command DTO for evaluation ratings with validation.
/// </summary>
public class EvaluationRatingCommandDto
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
