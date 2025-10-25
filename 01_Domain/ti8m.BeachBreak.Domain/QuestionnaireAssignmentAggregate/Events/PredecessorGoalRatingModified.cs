using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a goal rating is modified during review meeting.
/// Tracks changes to achievement ratings and justifications.
/// </summary>
public record PredecessorGoalRatingModified(
    Guid SourceGoalId,
    CompletionRole ModifiedByRole,
    decimal? DegreeOfAchievement,
    string? Justification,
    string ChangeReason,
    DateTime ModifiedAt,
    Guid ModifiedByEmployeeId) : IDomainEvent;
