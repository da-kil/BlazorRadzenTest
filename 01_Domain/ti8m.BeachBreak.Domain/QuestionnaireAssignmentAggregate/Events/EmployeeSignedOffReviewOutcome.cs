using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.PhaseRecords;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when employee signs-off on review outcome.
/// Transitions from ReviewFinished to EmployeeReviewConfirmed state.
/// </summary>
public record EmployeeSignedOffReviewOutcome(
    Guid AggregateId,
    EmployeeConfirmationRecord Confirmation) : IDomainEvent;
