using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

public record ManagerCompletionConfirmed(
    DateTime ConfirmedDate,
    string ConfirmedBy) : IDomainEvent;
