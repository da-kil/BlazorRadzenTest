using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class GoalDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public CompletionRole AddedByRole { get; set; }
    public DateTime TimeframeFrom { get; set; }
    public DateTime TimeframeTo { get; set; }
    public string ObjectiveDescription { get; set; } = string.Empty;
    public string MeasurementMetric { get; set; } = string.Empty;
    public decimal WeightingPercentage { get; set; }
    public DateTime AddedAt { get; set; }
    public Guid AddedByEmployeeId { get; set; }
    public List<GoalModificationDto> Modifications { get; set; } = new();
}
