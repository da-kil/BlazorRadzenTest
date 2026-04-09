using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.PhaseRecords;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when an employee confirms the review outcome.
/// Transitions from ReviewFinished to EmployeeReviewConfirmed state.
/// Manager must then finalize the questionnaire.
/// </summary>
public record EmployeeConfirmedReviewOutcome(
    Guid AggregateId,
    EmployeeConfirmationRecord Confirmation) : IDomainEvent;
