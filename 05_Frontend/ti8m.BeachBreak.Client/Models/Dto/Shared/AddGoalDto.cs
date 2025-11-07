namespace ti8m.BeachBreak.Client.Models.Dto;

public class AddGoalDto
{
    public Guid QuestionId { get; set; }
    public DateTime TimeframeFrom { get; set; }
    public DateTime TimeframeTo { get; set; }
    public string ObjectiveDescription { get; set; } = string.Empty;
    public string MeasurementMetric { get; set; } = string.Empty;

    /// <summary>
    /// Weighting percentage (0-100). Optional during in-progress states.
    /// Should be set during InReview state by manager.
    /// </summary>
    public decimal? WeightingPercentage { get; set; }
}
