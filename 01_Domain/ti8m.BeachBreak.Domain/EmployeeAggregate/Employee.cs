using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate.Events;

namespace ti8m.BeachBreak.Domain.EmployeeAggregate;

public class Employee : AggregateRoot
{
    public string EmployeeId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Role { get; private set; }
    public string EMail { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public DateOnly? LastStartDate { get; private set; }
    public string ManagerId { get; private set; }
    public string LoginName { get; private set; }
    public int OrganizationNumber { get; private set; }
    public bool IsDeleted { get; private set; }
    public ApplicationRole ApplicationRole { get; private set; }

    private Employee() { }

    public Employee(
        Guid id,
        string employeeId,
        string firstName,
        string lastName,
        string role,
        string email,
        DateOnly startDate,
        DateOnly? endDate,
        DateOnly? lastStartDate,
        string managerId,
        string loginName,
        int organizationNumber,
        ApplicationRole applicationRole = ApplicationRole.Employee)
    {
        RaiseEvent(new EmployeeAdded(
            id,
            employeeId,
            firstName,
            lastName,
            role,
            email,
            startDate,
            endDate,
            lastStartDate,
            managerId,
            loginName,
            organizationNumber,
            applicationRole));
    }

    public void Delete()
    {
        if (!IsDeleted)
        {
            RaiseEvent(new EmployeeDeleted());
        }
    }

    public void Undelete()
    {
        if (IsDeleted)
        {
            RaiseEvent(new EmployeeUndeleted(
                EmployeeId,
                FirstName,
                LastName,
                Role,
                EMail,
                StartDate,
                EndDate,
                LastStartDate,
                ManagerId,
                LoginName,
                OrganizationNumber,
                ApplicationRole));
        }
    }

    public void ChangeDepartment(int organizationNumber)
    {
        if (OrganizationNumber != organizationNumber)
        {
            RaiseEvent(new EmployeeDepartmentChanged(organizationNumber));
        }
    }

    public void ChangeEmail(string email)
    {
        if (EMail != email)
        {
            RaiseEvent(new EmployeeEmailChanged(email));
        }
    }

    public void ChangeLoginName(string loginName)
    {
        if (LoginName != loginName)
        {
            RaiseEvent(new EmployeeLoginNameChanged(loginName));
        }
    }

    public void ChangeManager(string managerId)
    {
        if (ManagerId != managerId)
        {
            RaiseEvent(new EmployeeManagerChanged(managerId));
        }
    }

    public void ChangeName(string firstName, string lastName)
    {
        if (FirstName != firstName || LastName != lastName)
        {
            RaiseEvent(new EmployeeNameChanged(firstName, lastName));
        }
    }

    public void ChangeRole(string role)
    {
        if (Role != role)
        {
            RaiseEvent(new EmployeeRoleChanged(role));
        }
    }

    public DomainResult ChangeApplicationRole(
        ApplicationRole newRole,
        ApplicationRole requesterRole,
        Guid changedByUserId,
        string changedByUserName)
    {
        // Validate authorization using domain service
        var authResult = ApplicationRoleAuthorizationService.CanAssignRole(requesterRole, newRole);

        if (!authResult.IsSuccess)
        {
            return authResult;
        }

        if (ApplicationRole != newRole)
        {
            RaiseEvent(new EmployeeApplicationRoleChanged(
                ApplicationRole,
                newRole,
                changedByUserId,
                changedByUserName,
                DateTime.UtcNow));
        }

        return DomainResult.Success();
    }

    public void ChangeEndDate(DateOnly? endDate)
    {
        if (EndDate != endDate)
        {
            RaiseEvent(new EmployeeEndDateChanged(endDate));
        }
    }

    public void ChangeStartDate(DateOnly startDate)
    {
        if (StartDate != startDate)
        {
            RaiseEvent(new EmployeeStartDateChanged(startDate));
        }
    }

    public void Apply(EmployeeAdded @event)
    {
        Id = @event.AggregateId;
        EmployeeId = @event.EmployeeId;
        FirstName = @event.FirstName;
        LastName = @event.LastName;
        Role = @event.Role;
        EMail = @event.EMail;
        StartDate = @event.StartDate;
        EndDate = @event.EndDate;
        LastStartDate = @event.LastStartDate;
        ManagerId = @event.ManagerId;
        LoginName = @event.LoginName;
        OrganizationNumber = @event.OrganizationNumber;
        ApplicationRole = @event.ApplicationRole;
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

    public void Apply(EmployeeApplicationRoleChanged @event)
    {
        ApplicationRole = @event.NewRole;
    }
}
