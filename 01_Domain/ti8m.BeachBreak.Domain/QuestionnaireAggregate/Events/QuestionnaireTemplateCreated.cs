using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAggregate.Events;

public record QuestionnaireTemplateCreated(
    Guid AggregateId,
    string Name,
    string Description,
    string Category,
    List<QuestionSection> Sections,
    QuestionnaireSettings Settings,
    DateTime CreatedDate) : IDomainEvent;