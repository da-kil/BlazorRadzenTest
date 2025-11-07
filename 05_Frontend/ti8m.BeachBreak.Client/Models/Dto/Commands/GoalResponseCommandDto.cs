namespace ti8m.BeachBreak.Client.Models.Dto.Commands;

/// <summary>
/// Command DTO for goal question responses.
/// Strongly-typed replacement for nested goal dictionaries.
/// </summary>
public class GoalResponseCommandDto
{
    public List<GoalDataCommandDto> Goals { get; set; } = new();
    public List<PredecessorRatingCommandDto> PredecessorRatings { get; set; } = new();
    public Guid? PredecessorAssignmentId { get; set; }
}
