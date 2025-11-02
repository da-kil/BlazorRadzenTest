using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Application.Query.Projections;

/// <summary>
/// Read model representation of a goal rating in the projection.
/// Simplified version of the domain GoalRating entity for query purposes.
/// Includes snapshot of the original goal at time of rating.
/// </summary>
public class GoalRatingReadModel
{
    public Guid Id { get; set; } // Generated ID for this rating
    public Guid SourceAssignmentId { get; set; }
    public Guid SourceGoalId { get; set; }
    public Guid QuestionId { get; set; }
    public ApplicationRole RatedByRole { get; set; }
    public decimal DegreeOfAchievement { get; set; }
    public string Justification { get; set; } = string.Empty;
    public DateTime RatedAt { get; set; }
    public Guid RatedByEmployeeId { get; set; }
    public List<GoalRatingModificationReadModel> Modifications { get; set; } = new();

    // Snapshot fields - captured at rating time to preserve historical accuracy
    public string SnapshotObjectiveDescription { get; set; } = string.Empty;
    public DateTime SnapshotTimeframeFrom { get; set; }
    public DateTime SnapshotTimeframeTo { get; set; }
    public string SnapshotMeasurementMetric { get; set; } = string.Empty;
    public ApplicationRole SnapshotAddedByRole { get; set; }
    public decimal SnapshotWeightingPercentage { get; set; }
}
