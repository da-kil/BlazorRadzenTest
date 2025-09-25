using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.EmployeeAggregate.Events;

public record EmployeeNameChanged(
    string FirstName,
    string LastName) : IDomainEvent;