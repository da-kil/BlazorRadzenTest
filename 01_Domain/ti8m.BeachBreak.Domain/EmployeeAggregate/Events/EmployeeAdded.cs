using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.EmployeeAggregate.Events;

public record EmployeeAdded(
    Guid AggregateId,
    string EmployeeId,
    string FirstName,
    string LastName,
    string Role,
    string EMail,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateOnly? LastStartDate,
    string ManagerId,
    string LoginName,
    int OrganizationNumber) : IDomainEvent;