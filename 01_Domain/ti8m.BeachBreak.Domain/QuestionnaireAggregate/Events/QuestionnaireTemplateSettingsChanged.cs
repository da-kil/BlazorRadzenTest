using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAggregate.Events;

public record QuestionnaireTemplateSettingsChanged(
    Guid AggregateId,
    QuestionnaireSettings Settings,
    DateTime ModifiedDate) : IDomainEvent;