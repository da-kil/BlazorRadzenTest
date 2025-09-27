using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.OrganizationAggregate.Events;

public record ParentOrganizationChanged(Guid? ParentOrganizationId) : IDomainEvent;