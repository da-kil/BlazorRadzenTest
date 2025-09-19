using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Services;

namespace ti8m.BeachBreak.Services;

public class FakeAuthenticationService : IAuthenticationService
{
    private readonly CurrentUser _fakeUser = new()
    {
        EmployeeId = "b0f388c2-6294-4116-a8b2-eccafa29b3fb",
        Name = "John Smith",
        Email = "john.smith@company.com",
        Department = "Executive Management",
        Role = "Manager", // Can be "Employee", "Manager", "HR"
        Title = "CEO",
        Permissions = new List<string> { "ViewOwnQuestionnaires", "CompleteQuestionnaires" }
    };

    public Task<CurrentUser> GetCurrentUserAsync()
    {
        return Task.FromResult(_fakeUser);
    }

    public string GetCurrentEmployeeId()
    {
        return _fakeUser.EmployeeId;
    }

    public string GetCurrentUserRole()
    {
        return _fakeUser.Role;
    }

    public bool IsInRole(string role)
    {
        return string.Equals(_fakeUser.Role, role, StringComparison.OrdinalIgnoreCase);
    }

    // Helper method to simulate different user types for testing
    public void SetFakeUserRole(string role, List<string> permissions)
    {
        _fakeUser.Role = role;
        _fakeUser.Permissions = permissions;

        // Adjust permissions based on role
        switch (role.ToLower())
        {
            case "manager":
                _fakeUser.EmployeeId = "MGR001";
                _fakeUser.Name = "Jane Smith";
                _fakeUser.Email = "jane.smith@company.com";
                _fakeUser.Title = "Engineering Manager";
                _fakeUser.Permissions = new List<string>
                {
                    "ViewOwnQuestionnaires", "CompleteQuestionnaires",
                    "ViewTeamQuestionnaires", "ManageTeamAssignments",
                    "SendReminders", "ViewTeamAnalytics"
                };
                break;
            case "hr":
                _fakeUser.EmployeeId = "HR001";
                _fakeUser.Name = "Alice Johnson";
                _fakeUser.Email = "alice.johnson@company.com";
                _fakeUser.Department = "Human Resources";
                _fakeUser.Title = "HR Manager";
                _fakeUser.Permissions = new List<string>
                {
                    "ViewOwnQuestionnaires", "CompleteQuestionnaires",
                    "ViewAllQuestionnaires", "ManageQuestionnaires",
                    "CreateQuestionnaires", "ViewOrganizationAnalytics",
                    "ManageAssignments", "GenerateReports"
                };
                break;
            default: // Employee
                _fakeUser.Permissions = new List<string> { "ViewOwnQuestionnaires", "CompleteQuestionnaires" };
                break;
        }
    }
}