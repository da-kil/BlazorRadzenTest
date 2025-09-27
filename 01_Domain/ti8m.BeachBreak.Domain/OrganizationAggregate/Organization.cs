using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.OrganizationAggregate.Events;

namespace ti8m.BeachBreak.Domain.OrganizationAggregate;

public class Organization : AggregateRoot
{
    public string Number { get; private set; }
    public string? ManagerId { get; private set; }
    public Guid? ParentId { get; private set; }
    public string Name { get; private set; }
    public bool IsIgnored { get; private set; }
    public bool IsDeleted { get; private set; }

    private Organization()
    {
    }

    public Organization(Guid id, string number, string? managerId, Guid? parentId, string name)
    {
        RaiseEvent(new OrganizationCreated(id, number, managerId, parentId, name));
    }

    public void ChangeName(string name)
    {
        if (Name != name)
        {
            RaiseEvent(new OrganizationNameChanged(name));
        }
    }

    public void ChangeManager(string? managerId)
    {
        if (ManagerId != managerId)
        {
            RaiseEvent(new OrganizationManagerChanged(managerId));
        }
    }

    public void ChangeParentOrganization(Guid? parentId)
    {
        if (ParentId != parentId)
        {
            RaiseEvent(new ParentOrganizationChanged(parentId));
        }
    }

    public void Delete()
    {
        if (!IsDeleted)
        {
            RaiseEvent(new OrganizationDeleted());
        }
    }

    public void Undelete()
    {
        if (IsDeleted)
        {
            RaiseEvent(new OrganizationUndeleted(
                Number,
                ManagerId,
                ParentId,
                Name,
                IsDeleted,
                IsIgnored));
        }
    }

    public void Ignore()
    {
        if (!IsIgnored)
        {
            RaiseEvent(new OrganizationIgnored());
        }
    }

    public void Apply(OrganizationCreated organizationCreated)
    {
        Id = organizationCreated.AggregateId;
        Number = organizationCreated.Number;
        Name = organizationCreated.Name;
        ManagerId = organizationCreated.ManagerId;
        ParentId = organizationCreated.ParentId;
        IsDeleted = false;
        IsIgnored = false;
    }

    public void Apply(OrganizationNameChanged organizationNameChanged)
    {
        Name = organizationNameChanged.Name;
    }

    public void Apply(OrganizationManagerChanged organizationManagerChanged)
    {
        ManagerId = organizationManagerChanged.ManagerId;
    }

    public void Apply(ParentOrganizationChanged parentOrganizationChanged)
    {
        ParentId = parentOrganizationChanged.ParentOrganizationId;
    }

    public void Apply(OrganizationDeleted organizationDeleted)
    {
        IsDeleted = true;
    }

    public void Apply(OrganizationUndeleted organizationUndeleted)
    {
        IsDeleted = false;
    }

    public void Apply(OrganizationIgnored organizationIgnored)
    {
        IsIgnored = true;
    }
}
