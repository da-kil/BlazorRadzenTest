using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;

/// <summary>
/// Immutable data representation of QuestionSection for domain events.
/// Decouples event schema from entity structure to enable independent evolution.
/// Section IS the question - no nested question items.
/// </summary>
public record QuestionSectionData(
    Guid Id,
    Translation Title,
    Translation Description,
    int Order,
    bool IsRequired,
    CompletionRole CompletionRole,
    QuestionType Type,
    IQuestionConfiguration Configuration,
    bool IsInstanceSpecific = false);
