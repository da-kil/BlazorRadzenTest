using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when employee feedback is unlinked from a question within an assignment.
/// </summary>
public record EmployeeFeedbackUnlinkedFromAssignment(
    Guid FeedbackId,
    Guid QuestionId,
    ApplicationRole UnlinkedByRole,
    DateTime UnlinkedAt,
    Guid UnlinkedByEmployeeId) : IDomainEvent;
