using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Query.Projections;

/// <summary>
/// Read model representation of a goal modification record.
/// </summary>
public class GoalModificationReadModel
{
    public DateTime? TimeframeFrom { get; set; }
    public DateTime? TimeframeTo { get; set; }
    public string? ObjectiveDescription { get; set; }
    public string? MeasurementMetric { get; set; }
    public decimal? WeightingPercentage { get; set; }
    public CompletionRole ModifiedByRole { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public Guid ModifiedByEmployeeId { get; set; }
}
