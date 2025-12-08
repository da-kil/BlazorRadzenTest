using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

/// <summary>
/// Immutable data representation of QuestionItem for domain events.
/// Decouples event schema from entity structure to enable independent evolution.
/// Uses strongly-typed IQuestionConfiguration for type safety.
/// </summary>
public record QuestionItemData(
    Guid Id,
    Translation Title,
    Translation Description,
    QuestionType Type,
    int Order,
    bool IsRequired,
    IQuestionConfiguration? Configuration);
