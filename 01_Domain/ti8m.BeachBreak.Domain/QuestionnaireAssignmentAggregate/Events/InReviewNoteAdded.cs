using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a note is added during the InReview phase
/// </summary>
public record InReviewNoteAdded(
    Guid NoteId,
    string Content,
    DateTime Timestamp,
    Guid? SectionId,
    Guid AuthorEmployeeId) : IDomainEvent;