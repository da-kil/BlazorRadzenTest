using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class GoalRatingDto
{
    public Guid Id { get; set; }
    public Guid SourceAssignmentId { get; set; }
    public Guid SourceGoalId { get; set; }
    public Guid QuestionId { get; set; }
    public CompletionRole RatedByRole { get; set; }
    public decimal DegreeOfAchievement { get; set; }
    public string Justification { get; set; } = string.Empty;

    // Snapshot of the original goal
    public string OriginalObjectiveDescription { get; set; } = string.Empty;
    public DateTime OriginalTimeframeFrom { get; set; }
    public DateTime OriginalTimeframeTo { get; set; }
    public string OriginalMeasurementMetric { get; set; } = string.Empty;
    public CompletionRole OriginalAddedByRole { get; set; }
    public decimal OriginalWeightingPercentage { get; set; }
}
