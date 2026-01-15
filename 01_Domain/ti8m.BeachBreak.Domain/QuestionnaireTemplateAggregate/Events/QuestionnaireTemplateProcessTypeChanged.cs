using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

public record QuestionnaireTemplateProcessTypeChanged(QuestionnaireProcessType ProcessType) : IDomainEvent;
