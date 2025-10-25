namespace ti8m.BeachBreak.CommandApi.Dto;

public class AddGoalDto
{
    public Guid QuestionId { get; set; }
    public string AddedByRole { get; set; } = string.Empty; // "Employee" or "Manager"
    public DateTime TimeframeFrom { get; set; }
    public DateTime TimeframeTo { get; set; }
    public string ObjectiveDescription { get; set; } = string.Empty;
    public string MeasurementMetric { get; set; } = string.Empty;
    public decimal WeightingPercentage { get; set; }
}
