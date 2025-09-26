using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAggregate.Events;

public record QuestionnaireTemplatePublished(
    Guid AggregateId,
    string PublishedBy,
    DateTime PublishedDate,
    DateTime LastPublishedDate) : IDomainEvent;