using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.CommandApi.DTOs;

/// <summary>
/// DTO representing individual goal data with type safety.
/// Replaces magic string dictionary keys with strongly-typed properties.
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