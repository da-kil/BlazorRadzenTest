using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a goal from a predecessor questionnaire is rated.
/// Employee and Manager rate goals separately during in-progress states.
/// </summary>
public record PredecessorGoalRated(
    Guid QuestionId,
    Guid SourceAssignmentId,
    Guid SourceGoalId,
    CompletionRole RatedByRole,
    decimal DegreeOfAchievement,
    string Justification,
    DateTime RatedAt,
    Guid RatedByEmployeeId) : IDomainEvent;
