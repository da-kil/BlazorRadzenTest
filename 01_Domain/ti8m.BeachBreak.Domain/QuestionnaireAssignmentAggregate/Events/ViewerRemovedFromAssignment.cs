using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a viewer is removed from a questionnaire assignment.
/// This revokes read-only access previously granted to the viewer.
/// </summary>
public record ViewerRemovedFromAssignment(
    Guid ViewerEmployeeId,
    DateTime RemovedDate,
    Guid RemovedByEmployeeId) : IDomainEvent;