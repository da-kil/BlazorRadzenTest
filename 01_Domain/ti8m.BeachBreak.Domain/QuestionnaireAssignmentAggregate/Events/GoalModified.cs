using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a goal is modified during in-progress or review states.
/// Tracks changes to goal definition for audit purposes.
/// Change reason is optional during in-progress states, required during review.
/// </summary>
public record GoalModified(
    Guid GoalId,
    DateTime? TimeframeFrom,
    DateTime? TimeframeTo,
    string? ObjectiveDescription,
    string? MeasurementMetric,
    decimal? WeightingPercentage,
    ApplicationRole ModifiedByRole,
    string? ChangeReason,
    DateTime ModifiedAt,
    Guid ModifiedByEmployeeId) : IDomainEvent;
