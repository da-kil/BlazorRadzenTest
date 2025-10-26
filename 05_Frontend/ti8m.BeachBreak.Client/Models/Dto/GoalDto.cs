namespace ti8m.BeachBreak.Client.Models.Dto;

public class GoalDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string AddedByRole { get; set; } = string.Empty; // CompletionRole as string for JSON
    public DateTime TimeframeFrom { get; set; }
    public DateTime TimeframeTo { get; set; }
    public string ObjectiveDescription { get; set; } = string.Empty;
    public string MeasurementMetric { get; set; } = string.Empty;
    public decimal WeightingPercentage { get; set; }
    public DateTime AddedAt { get; set; }
    public Guid AddedByEmployeeId { get; set; }
    public List<GoalModificationDto> Modifications { get; set; } = new();
}
