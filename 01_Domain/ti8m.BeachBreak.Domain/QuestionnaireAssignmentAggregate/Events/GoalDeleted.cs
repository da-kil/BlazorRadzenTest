using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a goal is deleted from the questionnaire assignment.
/// </summary>
public record GoalDeleted(
    Guid GoalId,
    DateTime DeletedAt,
    Guid DeletedByEmployeeId) : IDomainEvent;
