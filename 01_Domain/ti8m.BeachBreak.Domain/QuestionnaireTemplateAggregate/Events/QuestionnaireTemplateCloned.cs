using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

public record QuestionnaireTemplateCloned(
    Guid NewTemplateId,
    Guid SourceTemplateId,
    Translation Name,
    Translation Description,
    Guid CategoryId,
    bool RequiresManagerReview,
    bool IsCustomizable,
    bool AutoInitialize,
    List<QuestionSectionData> Sections,
    DateTime CreatedDate
) : IDomainEvent
{
    public Guid AggregateId => NewTemplateId;
}
