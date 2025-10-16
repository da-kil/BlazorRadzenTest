using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.EmployeeAggregate.Events;

public record EmployeeApplicationRoleChanged(
    ApplicationRole OldRole,
    ApplicationRole NewRole,
    Guid ChangedByUserId,
    string ChangedByUserName,
    DateTime ChangedAt) : IDomainEvent;
