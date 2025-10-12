using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a manager edits any answer during the review meeting.
/// Manager can edit employee sections, manager sections, and "both" sections.
/// </summary>
public record ManagerEditedAnswerDuringReview(
    Guid AggregateId,
    Guid SectionId,
    Guid QuestionId,
    CompletionRole OriginalCompletionRole,  // Who was supposed to complete this originally
    string NewAnswer,
    DateTime EditedDate,
    string EditedBy
) : IDomainEvent;
