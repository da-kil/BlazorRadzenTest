using ti8m.BeachBreak.Domain.OrganizationAggregate.Events;

namespace ti8m.BeachBreak.Application.Query.Projections;

public class OrganizationReadModel
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? ManagerId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsIgnored { get; set; }
    public bool IsDeleted { get; set; }

    // Apply methods for all Organization domain events
    public void Apply(OrganizationCreated @event)
    {
        Id = @event.AggregateId;
        Number = @event.Number;
        Name = @event.Name;
        ManagerId = @event.ManagerId;
        ParentId = @event.ParentId;
        IsDeleted = false;
        IsIgnored = false;
    }

    public void Apply(OrganizationNameChanged @event)
    {
        Name = @event.Name;
    }

    public void Apply(OrganizationManagerChanged @event)
    {
        ManagerId = @event.ManagerId;
    }

    public void Apply(ParentOrganizationChanged @event)
    {
        ParentId = @event.ParentOrganizationId;
    }

    public void Apply(OrganizationDeleted @event)
    {
        IsDeleted = true;
    }

    public void Apply(OrganizationUndeleted @event)
    {
        IsDeleted = false;
    }

    public void Apply(OrganizationIgnored @event)
    {
        IsIgnored = true;
    }
}