namespace ti8m.BeachBreak.Client.Models.DTOs;

/// <summary>
/// Simple DTO for goal question responses.
/// </summary>
public class GoalResponseDto
{
    public List<GoalDataDto> Goals { get; set; } = new();
    public List<PredecessorRatingDto> PredecessorRatings { get; set; } = new();
    public Guid? PredecessorAssignmentId { get; set; }
}