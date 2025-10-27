namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

/// <summary>
/// Immutable data representation of QuestionItem for domain events.
/// Decouples event schema from entity structure to enable independent evolution.
/// </summary>
public record QuestionItemData(
    Guid Id,
    string Title,
    string Description,
    QuestionType Type,
    int Order,
    bool IsRequired,
    Dictionary<string, object>? Configuration);
