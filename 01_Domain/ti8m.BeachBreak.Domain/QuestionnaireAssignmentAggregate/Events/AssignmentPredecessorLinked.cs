using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when an entire assignment links to a predecessor assignment
/// for assignment-wide goal access and reference.
/// </summary>
public record AssignmentPredecessorLinked(
    Guid PredecessorAssignmentId,
    ApplicationRole LinkedByRole,
    DateTime LinkedAt,
    Guid LinkedByEmployeeId) : IDomainEvent;