using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when an employee confirms the review outcome.
/// Employee cannot reject but can add comments about the review.
/// Transitions from ManagerReviewConfirmed to EmployeeReviewConfirmed state.
/// Manager must then finalize the questionnaire.
/// </summary>
public record EmployeeConfirmedReviewOutcome(
    Guid AggregateId,
    DateTime ConfirmedDate,
    string ConfirmedBy,
    string? EmployeeComments
) : IDomainEvent;
