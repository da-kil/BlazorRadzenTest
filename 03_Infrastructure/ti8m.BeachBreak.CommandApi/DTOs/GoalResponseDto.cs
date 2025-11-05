namespace ti8m.BeachBreak.CommandApi.DTOs;

/// <summary>
/// DTO for goal question responses including goals and predecessor ratings.
/// Eliminates nested dictionaries with strongly-typed goal data.
/// </summary>
public class GoalResponseDto
{
    public List<GoalDataDto> Goals { get; set; } = new();
    public List<PredecessorRatingDto> PredecessorRatings { get; set; } = new();
    public Guid? PredecessorAssignmentId { get; set; }

}