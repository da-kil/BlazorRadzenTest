namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

/// <summary>
/// Immutable data representation of QuestionSection for domain events.
/// Decouples event schema from entity structure to enable independent evolution.
/// </summary>
public record QuestionSectionData(
    Guid Id,
    Translation Title,
    Translation Description,
    int Order,
    bool IsRequired,
    CompletionRole CompletionRole,
    List<QuestionItemData> Questions);
