using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a viewer is added to a questionnaire assignment.
/// Viewers have read-only access to the assignment for collaboration, mentoring, or oversight purposes.
/// </summary>
public record ViewerAddedToAssignment(
    Guid ViewerEmployeeId,
    string ViewerEmployeeName,
    string ViewerEmployeeEmail,
    DateTime AddedDate,
    Guid AddedByEmployeeId) : IDomainEvent;