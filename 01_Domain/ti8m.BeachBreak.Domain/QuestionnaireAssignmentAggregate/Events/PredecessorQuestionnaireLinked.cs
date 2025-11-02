using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a goal question links to a predecessor questionnaire
/// for rating previous goals.
/// </summary>
public record PredecessorQuestionnaireLinked(
    Guid PredecessorAssignmentId,
    Guid QuestionId,
    ApplicationRole LinkedByRole,
    DateTime LinkedAt,
    Guid LinkedByEmployeeId) : IDomainEvent;
