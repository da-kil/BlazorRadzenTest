using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Services;

/// <summary>
/// Mapper for converting between domain entities and event data representations.
/// Keeps mapping logic separate from aggregate business logic.
/// </summary>
public static class QuestionnaireTemplateEventDataMapper
{
    /// <summary>
    /// Maps domain entities to event data for persistence in event stream.
    /// Section IS the question - no nested question items.
    /// </summary>
    public static List<QuestionSectionData> MapSectionsToData(List<QuestionSection> sections)
    {
        return sections.Select(s => new QuestionSectionData(
            s.Id,
            s.Title,
            s.Description,
            s.Order,
            s.CompletionRole,
            s.Type,
            s.Configuration
        )).ToList();
    }

    /// <summary>
    /// Maps event data back to domain entities for aggregate reconstitution.
    /// </summary>
    public static List<QuestionSection> MapDataToSections(List<QuestionSectionData> data)
    {
        return data.Select(s => new QuestionSection(
            s.Id,
            s.Title,
            s.Description,
            s.Order,
            s.CompletionRole,
            s.Type,
            s.Configuration
        )).ToList();
    }
}
