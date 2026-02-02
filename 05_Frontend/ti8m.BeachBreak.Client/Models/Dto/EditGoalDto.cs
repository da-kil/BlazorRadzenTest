namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO for editing a single goal during review.
/// Maps to the backend EditGoalDto for individual goal updates.
/// </summary>
public class EditGoalDto
{
    public Guid SectionId { get; set; }
    public Guid QuestionId { get; set; }
    public string OriginalCompletionRole { get; set; } = "";
    public string ObjectiveDescription { get; set; } = "";
    public string MeasurementMetric { get; set; } = "";
    public DateTime TimeframeFrom { get; set; }
    public DateTime TimeframeTo { get; set; }
    public decimal WeightingPercentage { get; set; }
}