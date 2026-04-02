using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate.Events;

namespace ti8m.BeachBreak.Domain.EmployeeAggregate;

public partial class Employee : AggregateRoot
{
    public Identity Identity { get; private set; }
    public PersonName Name { get; private set; }
    public string Role { get; private set; }
    public string EMail { get; private set; }
    public EmploymentPeriod Employment { get; private set; }
    public string LoginName { get; private set; }
    public bool IsDeleted { get; private set; }
    public ApplicationRole ApplicationRole { get; private set; }
    public Language PreferredLanguage { get; private set; }

    // Convenience passthroughs
    public string EmployeeId => Identity.EmployeeId;
    public string FirstName => Name.FirstName;
    public string LastName => Name.LastName;
    public string ManagerId => Identity.ManagerId;
    public int OrganizationNumber => Identity.OrganizationNumber;
    public DateOnly StartDate => Employment.StartDate;
    public DateOnly? EndDate => Employment.EndDate;
    public DateOnly? LastStartDate => Employment.LastStartDate;

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
        ApplicationRole applicationRole = ApplicationRole.Employee,
        Language preferredLanguage = Language.English)
    {
        RaiseEvent(new EmployeeAdded(
            id,
            new Identity(employeeId, managerId, organizationNumber),
            new PersonName(firstName, lastName),
            role,
            email,
            new EmploymentPeriod(startDate, endDate, lastStartDate),
            loginName,
            applicationRole,
            preferredLanguage));
    }

    public void Delete()
    {
        if (!IsDeleted)
            RaiseEvent(new EmployeeDeleted());
    }

    public void Undelete()
    {
        if (IsDeleted)
        {
            RaiseEvent(new EmployeeUndeleted(
                Identity,
                Name,
                Role,
                EMail,
                Employment,
                LoginName,
                ApplicationRole,
                PreferredLanguage));
        }
    }

    public void ChangeDepartment(int organizationNumber)
    {
        if (Identity.OrganizationNumber != organizationNumber)
            RaiseEvent(new EmployeeDepartmentChanged(organizationNumber));
    }

    public void ChangeEmail(string email)
    {
        if (EMail != email)
            RaiseEvent(new EmployeeEmailChanged(email));
    }

    public void ChangeLoginName(string loginName)
    {
        if (LoginName != loginName)
            RaiseEvent(new EmployeeLoginNameChanged(loginName));
    }

    public void ChangeManager(string managerId)
    {
        if (Identity.ManagerId != managerId)
            RaiseEvent(new EmployeeManagerChanged(managerId));
    }

    public void ChangeName(string firstName, string lastName)
    {
        if (Name.FirstName != firstName || Name.LastName != lastName)
            RaiseEvent(new EmployeeNameChanged(new PersonName(firstName, lastName)));
    }

    public void ChangeRole(string role)
    {
        if (Role != role)
            RaiseEvent(new EmployeeRoleChanged(role));
    }

    public DomainResult ChangeApplicationRole(
        ApplicationRole newRole,
        ApplicationRole requesterRole,
        Guid changedByUserId,
        string changedByUserName)
    {
        var authResult = ApplicationRoleAuthorizationService.CanAssignRole(requesterRole, newRole);
        if (!authResult.IsSuccess)
            return authResult;

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
        if (Employment.EndDate != endDate)
            RaiseEvent(new EmployeeEndDateChanged(endDate));
    }

    public void ChangeStartDate(DateOnly startDate)
    {
        if (Employment.StartDate != startDate)
            RaiseEvent(new EmployeeStartDateChanged(startDate));
    }

    public void ChangePreferredLanguage(Language preferredLanguage)
    {
        if (PreferredLanguage != preferredLanguage)
            RaiseEvent(new EmployeePreferredLanguageChanged(preferredLanguage));
    }

    public void Apply(EmployeeAdded @event)
    {
        Id = @event.AggregateId;
        Identity = @event.Identity;
        Name = @event.Name;
        Role = @event.Role;
        EMail = @event.EMail;
        Employment = @event.Employment;
        LoginName = @event.LoginName;
        ApplicationRole = @event.ApplicationRole;
        IsDeleted = false;
    }

    public void Apply(EmployeeDeleted @event) => IsDeleted = true;

    public void Apply(EmployeeUndeleted @event)
    {
        Identity = @event.Identity;
        Name = @event.Name;
        Role = @event.Role;
        EMail = @event.EMail;
        Employment = @event.Employment;
        LoginName = @event.LoginName;
        ApplicationRole = @event.ApplicationRole;
        PreferredLanguage = @event.PreferredLanguage;
        IsDeleted = false;
    }

    public void Apply(EmployeeDepartmentChanged @event)
    {
        Identity = new Identity(Identity.EmployeeId, Identity.ManagerId, @event.OrganizationNumber);
    }

    public void Apply(EmployeeEmailChanged @event) => EMail = @event.Email;

    public void Apply(EmployeeLoginNameChanged @event) => LoginName = @event.LoginName;

    public void Apply(EmployeeManagerChanged @event)
    {
        Identity = new Identity(Identity.EmployeeId, @event.ManagerId, Identity.OrganizationNumber);
    }

    public void Apply(EmployeeNameChanged @event) => Name = @event.Name;

    public void Apply(EmployeeRoleChanged @event) => Role = @event.Role;

    public void Apply(EmployeeEndDateChanged @event)
    {
        Employment = new EmploymentPeriod(Employment.StartDate, @event.EndDate, Employment.LastStartDate);
    }

    public void Apply(EmployeeStartDateChanged @event)
    {
        Employment = new EmploymentPeriod(@event.StartDate, Employment.EndDate, Employment.LastStartDate);
    }

    public void Apply(EmployeeApplicationRoleChanged @event) => ApplicationRole = @event.NewRole;

    public void Apply(EmployeePreferredLanguageChanged @event) => PreferredLanguage = @event.PreferredLanguage;
}
