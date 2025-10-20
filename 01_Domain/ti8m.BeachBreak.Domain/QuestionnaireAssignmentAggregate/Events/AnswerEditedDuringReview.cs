using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

public record AnswerEditedDuringReview(
    Guid SectionId,
    Guid QuestionId,
    string Answer,
    DateTime EditedDate,
    Guid EditedByEmployeeId) : IDomainEvent;
