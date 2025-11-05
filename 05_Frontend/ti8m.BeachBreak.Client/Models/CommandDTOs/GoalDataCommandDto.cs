namespace ti8m.BeachBreak.Client.Models.CommandDTOs;

/// <summary>
/// Command DTO for individual goal data.
/// Eliminates magic string keys with strongly-typed properties.
/// </summary>
public class GoalDataCommandDto
{
    public Guid GoalId { get; set; }
    public string ObjectiveDescription { get; set; } = string.Empty;
    public DateTime TimeframeFrom { get; set; }
    public DateTime TimeframeTo { get; set; }
    public string MeasurementMetric { get; set; } = string.Empty;
    public decimal WeightingPercentage { get; set; }
    public ApplicationRole AddedByRole { get; set; }

    /// <summary>
    /// Validates that this goal has all required data.
    /// </summary>
    public bool IsValid =>
        GoalId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(ObjectiveDescription) &&
        !string.IsNullOrWhiteSpace(MeasurementMetric) &&
        TimeframeFrom < TimeframeTo &&
        WeightingPercentage >= 0 && WeightingPercentage <= 100;
}