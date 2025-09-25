using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.EmployeeAggregate.Events;

public record EmployeeEndDateChanged(DateOnly? EndDate) : IDomainEvent;