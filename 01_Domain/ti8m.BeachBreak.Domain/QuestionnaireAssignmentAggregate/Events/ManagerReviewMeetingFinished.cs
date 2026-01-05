using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a manager finishes the review meeting.
/// Transitions from InReview to ReviewFinished state.
/// Employee must then confirm the review outcome.
/// </summary>
public record ManagerReviewMeetingFinished(
    Guid AggregateId,
    DateTime FinishedDate,
    Guid FinishedByEmployeeId,
    string? ReviewSummary
) : IDomainEvent;
