using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.CommandApi.DTOs;

/// <summary>
/// DTO representing a rating of a goal from a previous questionnaire.
/// Provides type safety for predecessor goal evaluation data.
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