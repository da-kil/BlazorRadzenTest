using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Models.DTOs;

/// <summary>
/// Simple DTO for predecessor goal ratings.
/// </summary>
public class PredecessorRatingDto
{
    public Guid SourceGoalId { get; set; }
    public int DegreeOfAchievement { get; set; }
    public string Justification { get; set; } = string.Empty;
    public ApplicationRole RatedByRole { get; set; }
    public string OriginalObjective { get; set; } = string.Empty;
    public ApplicationRole OriginalAddedByRole { get; set; }
}