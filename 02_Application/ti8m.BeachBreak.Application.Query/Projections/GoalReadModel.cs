using ti8m.BeachBreak.Application.Query.Models;

namespace ti8m.BeachBreak.Application.Query.Projections;

/// <summary>
/// Read model representation of a goal in the projection.
/// Simplified version of the domain Goal entity for query purposes.
/// </summary>
public class GoalReadModel
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public ApplicationRole AddedByRole { get; set; }
    public DateTime TimeframeFrom { get; set; }
    public DateTime TimeframeTo { get; set; }
    public string ObjectiveDescription { get; set; } = string.Empty;
    public string MeasurementMetric { get; set; } = string.Empty;
    public decimal WeightingPercentage { get; set; }
    public DateTime AddedAt { get; set; }
    public Guid AddedByEmployeeId { get; set; }
    public List<GoalModificationReadModel> Modifications { get; set; } = new();
}
