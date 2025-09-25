using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate.Events;

namespace ti8m.BeachBreak.Domain.EmployeeAggregate;

public class Employee : AggregateRoot
{
    public string FirstName { get; private set;}
    public string LastName { get; private set;}
    public string Role { get; private set;}
    public string EMail { get; private set;}
    public DateOnly StartDate { get; private set;}
    public DateOnly? EndDate { get; private set;}
    public DateOnly? LastStartDate { get; private set;}
    public Guid? ManagerId { get; private set;}
    public string Manager { get; private set;}
    public string LoginName { get; private set;}
    public string EmployeeNumber { get; private set;}
    public int OrganizationNumber { get; private set;}
    public string Organization { get; private set;}
    public bool IsDeleted { get; private set;}

    private Employee() { }

    public Employee(
        Guid id,
        string firstName,
        string lastName,
        string role,
        string email,
        DateOnly startDate,
        DateOnly? endDate,
        DateOnly? lastStartDate,
        Guid? managerId,
        string manager,
        string loginName,
        string employeeNumber,
        int organizationNumber,
        string organization)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        EMail = email;
        StartDate = startDate;
        EndDate = endDate;
        LastStartDate = lastStartDate;
        ManagerId = managerId;
        Manager = manager;
        LoginName = loginName;
        EmployeeNumber = employeeNumber;
        OrganizationNumber = organizationNumber;
        Organization = organization;
    }

    public void Apply(EmployeeAdded @event)
    {
        Id = @event.AggregateId;
        FirstName = @event.FirstName;
        LastName = @event.LastName;
        Role = @event.Role;
        EMail = @event.EMail;
        StartDate = @event.StartDate;
        EndDate = @event.EndDate;
        LastStartDate = @event.LastStartDate;
        ManagerId = @event.ManagerId;
        LoginName = @event.LoginName;
        EmployeeNumber = @event.EmployeeNumber;
        OrganizationNumber = @event.OrganizationNumber;
        Organization = @event.Organization;
        IsDeleted = false;
    }

    public void Apply(EmployeeDeleted @event)
    {
        IsDeleted = true;
    }

    public void Apply(EmployeeUndeleted @event)
    {
        IsDeleted = false;
    }

    public void Apply(EmployeeDepartmentChanged @event)
    {
        OrganizationNumber = @event.OrganizationNumber;
        Organization = @event.Organization;
    }

    public void Apply(EmployeeEmailChanged @event)
    {
        EMail = @event.Email;
    }

    public void Apply(EmployeeLoginNameChanged @event)
    {
        LoginName = @event.LoginName;
    }

    public void Apply(EmployeeManagerChanged @event)
    {
        ManagerId = @event.ManagerId;
    }

    public void Apply(EmployeeNameChanged @event)
    {
        FirstName = @event.FirstName;
        LastName = @event.LastName;
    }

    public void Apply(EmployeeRoleChanged @event)
    {
        Role = @event.Role;
    }

    public void Apply(EmployeeEndDateChanged @event)
    {
        EndDate = @event.EndDate;
    }

    public void Apply(EmployeeStartDateChanged @event)
    {
        StartDate = @event.StartDate;
    }
}
