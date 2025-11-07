using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.CommandApi.Dto;

public class ModifyGoalDto
{
    public DateTime? TimeframeFrom { get; set; }
    public DateTime? TimeframeTo { get; set; }
    public string? ObjectiveDescription { get; set; }
    public string? MeasurementMetric { get; set; }
    public decimal? WeightingPercentage { get; set; }
    public string? ChangeReason { get; set; } // Required only during InReview state
}
