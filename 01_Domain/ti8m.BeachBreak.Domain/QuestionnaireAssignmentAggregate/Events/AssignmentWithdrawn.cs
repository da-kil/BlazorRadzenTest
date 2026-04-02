using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.PhaseRecords;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

public record AssignmentWithdrawn(WithdrawalRecord Withdrawal) : IDomainEvent;
