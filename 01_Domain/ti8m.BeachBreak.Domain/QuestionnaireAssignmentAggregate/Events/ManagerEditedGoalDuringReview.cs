using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a manager edits a specific goal during the review process.
/// </summary>
public record ManagerEditedGoalDuringReview(
    Guid AggregateId,
    Guid GoalId,
    Guid SectionId,
    Guid QuestionId,
    ApplicationRole OriginalCompletionRole,
    DateTime EditedDate,
    Guid EditedByEmployeeId
) : IDomainEvent;