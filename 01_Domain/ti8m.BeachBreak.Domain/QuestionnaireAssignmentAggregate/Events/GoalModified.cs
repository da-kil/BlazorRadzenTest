using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a goal is modified during review meeting.
/// Tracks changes to goal definition for audit purposes.
/// </summary>
public record GoalModified(
    Guid GoalId,
    DateTime? TimeframeFrom,
    DateTime? TimeframeTo,
    string? ObjectiveDescription,
    string? MeasurementMetric,
    decimal? WeightingPercentage,
    CompletionRole ModifiedByRole,
    string ChangeReason,
    DateTime ModifiedAt,
    Guid ModifiedByEmployeeId) : IDomainEvent;
