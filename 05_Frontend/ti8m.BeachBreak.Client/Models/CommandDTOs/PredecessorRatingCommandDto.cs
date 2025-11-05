namespace ti8m.BeachBreak.Client.Models.CommandDTOs;

/// <summary>
/// Command DTO for predecessor goal ratings.
/// Strongly-typed replacement for rating dictionaries.
/// </summary>
public class PredecessorRatingCommandDto
{
    public Guid SourceGoalId { get; set; }
    public int DegreeOfAchievement { get; set; }
    public string Justification { get; set; } = string.Empty;
    public ApplicationRole RatedByRole { get; set; }
    public string OriginalObjective { get; set; } = string.Empty;
    public ApplicationRole OriginalAddedByRole { get; set; }
}