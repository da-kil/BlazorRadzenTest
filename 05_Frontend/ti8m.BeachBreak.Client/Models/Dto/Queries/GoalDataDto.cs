using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Models.DTOs;

/// <summary>
/// Simple DTO for goal data.
/// </summary>
public class GoalDataDto
{
    public Guid GoalId { get; set; }
    public string ObjectiveDescription { get; set; } = string.Empty;
    public DateTime TimeframeFrom { get; set; }
    public DateTime TimeframeTo { get; set; }
    public string MeasurementMetric { get; set; } = string.Empty;
    public decimal WeightingPercentage { get; set; }
    public ApplicationRole AddedByRole { get; set; }
}