namespace ti8m.BeachBreak.Client.Models.Dto;

public class ModifyGoalDto
{
    public DateTime? TimeframeFrom { get; set; }
    public DateTime? TimeframeTo { get; set; }
    public string? ObjectiveDescription { get; set; }
    public string? MeasurementMetric { get; set; }
    public decimal? WeightingPercentage { get; set; }
    public string? ChangeReason { get; set; } // Required only during InReview state
}
