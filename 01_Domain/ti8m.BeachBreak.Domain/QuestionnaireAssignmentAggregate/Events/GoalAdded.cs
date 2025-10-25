using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a new goal is added to the current questionnaire.
/// Goals are added dynamically during in-progress states, not defined in template.
/// </summary>
public record GoalAdded(
    Guid QuestionId,
    Guid GoalId,
    CompletionRole AddedByRole,
    DateTime TimeframeFrom,
    DateTime TimeframeTo,
    string ObjectiveDescription,
    string MeasurementMetric,
    decimal WeightingPercentage,
    DateTime AddedAt,
    Guid AddedByEmployeeId) : IDomainEvent;
