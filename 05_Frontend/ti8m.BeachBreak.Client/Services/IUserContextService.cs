using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface IUserContextService
{
    string CurrentUserId { get; }
    string CurrentEmployeeId { get; }
    UserRole CurrentRole { get; }
    string CurrentDepartment { get; }
    List<string> CurrentPermissions { get; }
    bool HasPermission(string permission);
    bool CanAccessEmployee(string employeeId);
    bool CanAccessDepartment(string department);
    Task<CurrentUser> GetCurrentUserAsync();
}

public class UserContextService : IUserContextService
{
    private readonly IAuthenticationService _authenticationService;
    private CurrentUser? _cachedUser;
    private readonly Dictionary<UserRole, List<string>> _rolePermissions;

    public UserContextService(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        _rolePermissions = new Dictionary<UserRole, List<string>>
        {
            [UserRole.Employee] = new List<string>
            {
                "questionnaire.view.own",
                "questionnaire.submit.own",
                "response.save.own"
            },
            [UserRole.Manager] = new List<string>
            {
                "questionnaire.view.own",
                "questionnaire.submit.own",
                "response.save.own",
                "questionnaire.view.team",
                "analytics.view.team",
                "reminder.send.team"
            },
            [UserRole.HR] = new List<string>
            {
                "questionnaire.view.all",
                "questionnaire.create",
                "questionnaire.assign",
                "analytics.view.organization",
                "reports.generate.all",
                "reminder.send.bulk"
            },
            [UserRole.Admin] = new List<string>
            {
                "questionnaire.view.all",
                "questionnaire.create",
                "questionnaire.assign",
                "questionnaire.delete",
                "analytics.view.organization",
                "reports.generate.all",
                "reminder.send.bulk",
                "user.manage",
                "system.configure"
            }
        };
    }

    public string CurrentUserId => GetCurrentUserAsync().Result.UserId;
    public string CurrentEmployeeId => GetCurrentUserAsync().Result.EmployeeId;
    public UserRole CurrentRole => GetCurrentUserAsync().Result.Role;
    public string CurrentDepartment => GetCurrentUserAsync().Result.Department ?? string.Empty;

    public List<string> CurrentPermissions =>
        _rolePermissions.TryGetValue(CurrentRole, out var permissions)
            ? permissions
            : new List<string>();

    public bool HasPermission(string permission)
    {
        return CurrentPermissions.Contains(permission);
    }

    public bool CanAccessEmployee(string employeeId)
    {
        // Employee can only access their own data
        if (CurrentRole == UserRole.Employee)
        {
            return CurrentEmployeeId == employeeId;
        }

        // Manager can access their team members
        if (CurrentRole == UserRole.Manager)
        {
            // TODO: Implement team member check via API call
            return true; // Placeholder - should check if employee is in manager's team
        }

        // HR and Admin can access all employees
        return CurrentRole == UserRole.HR || CurrentRole == UserRole.Admin;
    }

    public bool CanAccessDepartment(string department)
    {
        // Employee and Manager are limited to their department
        if (CurrentRole == UserRole.Employee || CurrentRole == UserRole.Manager)
        {
            return string.Equals(CurrentDepartment, department, StringComparison.OrdinalIgnoreCase);
        }

        // HR and Admin can access all departments
        return CurrentRole == UserRole.HR || CurrentRole == UserRole.Admin;
    }

    public async Task<CurrentUser> GetCurrentUserAsync()
    {
        if (_cachedUser == null)
        {
            _cachedUser = await _authenticationService.GetCurrentUserAsync();
        }
        return _cachedUser;
    }
}

public enum UserRole
{
    Employee,
    Manager,
    HR,
    Admin
}

public class CurrentUser
{
    public string UserId { get; set; } = "b0f388c2-6294-4116-a8b2-eccafa29b3fb";
    public string EmployeeId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? Department { get; set; }
    public string? ManagerId { get; set; }
    public List<string> TeamMemberIds { get; set; } = new();
}