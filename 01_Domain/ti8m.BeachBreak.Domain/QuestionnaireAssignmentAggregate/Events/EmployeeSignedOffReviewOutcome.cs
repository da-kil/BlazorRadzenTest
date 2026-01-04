using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when employee signs-off on review outcome.
/// Transitions from ReviewFinished to EmployeeReviewConfirmed state.
/// This is the intermediate sign-off step before final confirmation.
/// </summary>
public record EmployeeSignedOffReviewOutcome(
    Guid AggregateId,
    DateTime SignedOffDate,
    Guid SignedOffByEmployeeId,
    string? SignOffComments
) : IDomainEvent;