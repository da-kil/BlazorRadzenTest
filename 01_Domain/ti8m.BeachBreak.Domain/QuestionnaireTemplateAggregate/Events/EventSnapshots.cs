namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

/// <summary>
/// Immutable snapshot of QuestionItem for event sourcing.
/// Decouples event schema from entity structure to enable independent evolution.
/// </summary>
public record QuestionItemSnapshot(
    Guid Id,
    string Title,
    string Description,
    QuestionType Type,
    int Order,
    bool IsRequired,
    Dictionary<string, object>? Configuration);

/// <summary>
/// Immutable snapshot of QuestionSection for event sourcing.
/// Decouples event schema from entity structure to enable independent evolution.
/// </summary>
public record QuestionSectionSnapshot(
    Guid Id,
    string Title,
    string Description,
    int Order,
    bool IsRequired,
    CompletionRole CompletionRole,
    List<QuestionItemSnapshot> Questions);
