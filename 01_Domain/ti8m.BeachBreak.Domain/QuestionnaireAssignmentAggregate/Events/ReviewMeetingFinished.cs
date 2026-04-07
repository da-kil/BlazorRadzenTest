using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when the review meeting is finished.
/// Transitions from InReview to ReviewFinished state.
/// Employee must then confirm the review outcome.
/// </summary>
public record ReviewMeetingFinished(
    Guid AggregateId,
    DateTime FinishedDate,
    Guid FinishedByEmployeeId
) : IDomainEvent;
