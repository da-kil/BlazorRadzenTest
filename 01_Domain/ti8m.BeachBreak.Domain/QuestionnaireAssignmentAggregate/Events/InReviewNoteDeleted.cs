using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a note is deleted during the InReview phase
/// </summary>
public record InReviewNoteDeleted(
    Guid NoteId,
    Guid DeletedByEmployeeId) : IDomainEvent;