using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.Events;

/// <summary>
/// Domain event raised when a goal is edited during review.
/// Contains complete goal data for event sourcing reconstruction and audit trail.
/// </summary>
public record GoalEdited(
    Guid AggregateId,
    Guid GoalId,
    Guid SectionId,
    ApplicationRole OriginalCompletionRole,
    // New goal data - essential for event sourcing
    string NewObjectiveDescription,
    string NewMeasurementMetric,
    DateTime NewTimeframeFrom,
    DateTime NewTimeframeTo,
    decimal NewWeightingPercentage,
    // Audit information
    DateTime EditedDate,
    Guid EditedByEmployeeId
) : IDomainEvent;