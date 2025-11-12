namespace ti8m.BeachBreak.Client.Models.Dto.Shared;

/// <summary>
/// DTO for modifying an existing goal.
/// Used by NewGoalsCollaborativeSection for queuing goal modifications.
/// </summary>
public class ModifyGoalDto
{
    public string? ObjectiveDescription { get; set; }
    public string? MeasurementMetric { get; set; }
    public DateTime? TimeframeFrom { get; set; }
    public DateTime? TimeframeTo { get; set; }
    public decimal? WeightingPercentage { get; set; }
    public string? ChangeReason { get; set; }
}
