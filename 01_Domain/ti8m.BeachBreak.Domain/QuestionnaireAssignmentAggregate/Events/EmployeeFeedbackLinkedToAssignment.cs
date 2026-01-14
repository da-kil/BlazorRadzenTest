using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when employee feedback is linked to a question within an assignment.
/// </summary>
public record EmployeeFeedbackLinkedToAssignment(
    Guid FeedbackId,
    Guid QuestionId,
    ApplicationRole LinkedByRole,
    DateTime LinkedAt,
    Guid LinkedByEmployeeId) : IDomainEvent;
