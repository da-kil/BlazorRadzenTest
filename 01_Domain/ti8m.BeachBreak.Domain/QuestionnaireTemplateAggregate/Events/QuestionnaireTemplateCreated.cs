using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

public record QuestionnaireTemplateCreated(
    Guid AggregateId,
    string Name,
    string Description,
    Guid CategoryId,
    bool RequiresManagerReview,
    List<QuestionSection> Sections,
    QuestionnaireSettings Settings,
    DateTime CreatedDate) : IDomainEvent;