using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

public record QuestionnaireTemplatePublished(
    Guid PublishedByEmployeeId,
    DateTime PublishedDate,
    DateTime LastPublishedDate) : IDomainEvent;