using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.EmployeeAggregate.Events;

public record EmployeeAdded(
    Guid AggregateId,
    string FirstName,
    string LastName,
    string Role,
    string EMail,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateOnly? LastStartDate,
    Guid? ManagerId,
    string Manager,
    string LoginName,
    string EmployeeNumber,
    int OrganizationNumber,
    string Organization,
    DateTime CreatedDate) : IDomainEvent;