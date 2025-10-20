using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

public record AssignmentWithdrawn(
    DateTime WithdrawnDate,
    Guid WithdrawnByEmployeeId,
    string? WithdrawalReason) : IDomainEvent;