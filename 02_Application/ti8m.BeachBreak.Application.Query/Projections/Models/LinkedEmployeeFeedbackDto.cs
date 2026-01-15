using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.ValueObjects;

namespace ti8m.BeachBreak.Application.Query.Projections.Models;

/// <summary>
/// DTO for employee feedback records linked to questionnaire assignments.
/// Contains essential feedback information for display during questionnaire workflow.
/// </summary>
public record LinkedEmployeeFeedbackDto
{
    public Guid FeedbackId { get; init; }
    public Guid EmployeeId { get; init; }
    public FeedbackSourceType SourceType { get; init; }
    public string ProviderName { get; init; } = string.Empty;
    public DateTime FeedbackDate { get; init; }
    public ConfigurableFeedbackData FeedbackData { get; init; } = null!;
    public string? ProjectName { get; init; }
    public string? ProjectRole { get; init; }
    public decimal? AverageRating { get; init; }
    public int RatedItemsCount { get; init; }
    public bool HasComments { get; init; }
}
