using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

public record QuestionnaireTemplateCreated(
    Guid AggregateId,
    Translation Name,
    Translation Description,
    Guid CategoryId,
    QuestionnaireProcessType ProcessType,
    bool IsCustomizable,
    bool AutoInitialize,
    List<QuestionSectionData> Sections,
    DateTime CreatedDate) : IDomainEvent;