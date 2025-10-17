using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

public record QuestionnaireTemplateDescriptionChanged(string Description) : IDomainEvent;