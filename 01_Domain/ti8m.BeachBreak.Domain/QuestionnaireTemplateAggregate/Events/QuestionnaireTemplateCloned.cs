using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

public record QuestionnaireTemplateCloned(
    Guid NewTemplateId,
    Guid SourceTemplateId,
    string Name,
    string Description,
    Guid CategoryId,
    bool RequiresManagerReview,
    List<QuestionSection> Sections,
    DateTime CreatedDate
) : IDomainEvent
{
    public Guid AggregateId => NewTemplateId;
}
