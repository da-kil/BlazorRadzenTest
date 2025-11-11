namespace ti8m.BeachBreak.Client.Models.Dto.Shared;

/// <summary>
/// DTO for adding a new goal to a questionnaire assignment.
/// Used by NewGoalsCollaborativeSection for queuing goal additions.
/// </summary>
public class AddGoalDto
{
    public Guid QuestionId { get; set; }
    public string? ObjectiveDescription { get; set; }
    public string? MeasurementMetric { get; set; }
    public DateTime TimeframeFrom { get; set; }
    public DateTime TimeframeTo { get; set; }
    public decimal? WeightingPercentage { get; set; }
}
