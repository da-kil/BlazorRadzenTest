using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

public record ManagerSectionCompleted(
    Guid SectionId,
    DateTime CompletedDate) : IDomainEvent;
