using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class GoalRatingDto
{
    public Guid SourceAssignmentId { get; set; }
    public Guid SourceGoalId { get; set; }
    public ApplicationRole RatedByRole { get; set; }
    public int DegreeOfAchievement { get; set; }
    public string Justification { get; set; } = string.Empty;

    // Snapshot of the original goal
    public string OriginalObjective { get; set; } = string.Empty;
    public ApplicationRole OriginalAddedByRole { get; set; }
}
