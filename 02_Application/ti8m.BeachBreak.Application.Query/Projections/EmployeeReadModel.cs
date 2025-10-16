using ti8m.BeachBreak.Domain.EmployeeAggregate.Events;

namespace ti8m.BeachBreak.Application.Query.Projections;

public class EmployeeReadModel
{
    public Guid Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string EMail { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly? LastStartDate { get; set; }
    public string ManagerId { get; set; } = string.Empty;
    public string LoginName { get; set; } = string.Empty;
    public int OrganizationNumber { get; set; }
    public bool IsDeleted { get; set; }
    public Domain.EmployeeAggregate.ApplicationRole ApplicationRole { get; set; }

    // Apply methods for all Employee domain events
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
