using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Query.Projections;

/// <summary>
/// Read model representation of a goal rating modification record.
/// </summary>
public class GoalRatingModificationReadModel
{
    public decimal? DegreeOfAchievement { get; set; }
    public string? Justification { get; set; }
    public CompletionRole ModifiedByRole { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public DateTime ModifiedAt { get; set; }
    public Guid ModifiedByEmployeeId { get; set; }
}
