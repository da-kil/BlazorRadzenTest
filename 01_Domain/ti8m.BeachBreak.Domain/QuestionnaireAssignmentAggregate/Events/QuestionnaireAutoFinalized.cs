using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a questionnaire is automatically finalized after employee submission.
/// This occurs when the template does not require manager review.
/// Transitions from EmployeeSubmitted to Finalized state.
/// </summary>
public record QuestionnaireAutoFinalized(
    Guid AggregateId,
    DateTime FinalizedDate,
    Guid FinalizedByEmployeeId,
    string Reason
) : IDomainEvent;
