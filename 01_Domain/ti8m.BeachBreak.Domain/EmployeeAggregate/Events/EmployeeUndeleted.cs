using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.EmployeeAggregate.Events;

public record EmployeeUndeleted(
    Identity Identity,
    PersonName Name,
    string Role,
    string EMail,
    EmploymentPeriod Employment,
    string LoginName,
    ApplicationRole ApplicationRole,
    Language PreferredLanguage) : IDomainEvent;
