namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class GoalModificationDto
{
    public string ModifiedByRole { get; set; } = string.Empty; // String representation of CompletionRole enum
    public string ChangeReason { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public Guid ModifiedByEmployeeId { get; set; }
    public DateTime? TimeframeFrom { get; set; }
    public DateTime? TimeframeTo { get; set; }
    public string? ObjectiveDescription { get; set; }
    public string? MeasurementMetric { get; set; }
    public decimal? WeightingPercentage { get; set; }
}
