using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a goal question links to a predecessor questionnaire
/// for rating previous goals.
/// </summary>
public record PredecessorQuestionnaireLinked(
    Guid PredecessorAssignmentId,
    Guid QuestionId,
    CompletionRole LinkedByRole,
    DateTime LinkedAt,
    Guid LinkedByEmployeeId) : IDomainEvent;
