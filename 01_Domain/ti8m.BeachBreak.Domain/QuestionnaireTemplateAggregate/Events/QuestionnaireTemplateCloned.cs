using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

public record QuestionnaireTemplateCloned(
    Guid NewTemplateId,
    Guid SourceTemplateId,
    string Name,
    string Description,
    Guid CategoryId,
    List<QuestionSection> Sections,
    QuestionnaireSettings Settings,
    DateTime CreatedDate
) : IDomainEvent
{
    public Guid AggregateId => NewTemplateId;
}
