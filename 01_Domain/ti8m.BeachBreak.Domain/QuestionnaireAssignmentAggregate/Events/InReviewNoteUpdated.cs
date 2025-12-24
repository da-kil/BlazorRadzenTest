using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a note is updated during the InReview phase
/// </summary>
public record InReviewNoteUpdated(
    Guid NoteId,
    string Content,
    DateTime UpdatedAt,
    Guid UpdatedByEmployeeId) : IDomainEvent;