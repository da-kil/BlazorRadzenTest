using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

public record QuestionnaireTemplateCategoryChanged(
    Guid AggregateId,
    Guid CategoryId) : IDomainEvent;