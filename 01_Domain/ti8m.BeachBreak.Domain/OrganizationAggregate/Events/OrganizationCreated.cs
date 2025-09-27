using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.OrganizationAggregate.Events;

public record OrganizationCreated(
    Guid AggregateId,
    string Number,
    string? ManagerId,
    Guid? ParentId,
    string Name) : IDomainEvent;