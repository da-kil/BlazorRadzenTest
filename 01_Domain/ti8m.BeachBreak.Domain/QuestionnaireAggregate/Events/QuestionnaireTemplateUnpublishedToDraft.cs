using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAggregate.Events;

public record QuestionnaireTemplateUnpublishedToDraft(
    Guid AggregateId,
    DateTime ModifiedDate) : IDomainEvent;