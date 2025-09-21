using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Services;

namespace ti8m.BeachBreak.Services;

public class FakeAuthenticationService : IAuthenticationService
{
    private readonly CurrentUser _fakeUser = new()
    {
        UserId = "user-001",
        EmployeeId = "b0f388c2-6294-4116-a8b2-eccafa29b3fb",
        UserName = "John Smith",
        Email = "john.smith@company.com",
        Department = "Executive Management",
        Role = UserRole.Manager
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
        return _fakeUser.Role.ToString();
    }

    public bool IsInRole(string role)
    {
        return string.Equals(_fakeUser.Role.ToString(), role, StringComparison.OrdinalIgnoreCase);
    }

    // Helper method to simulate different user types for testing
    public void SetFakeUserRole(UserRole role)
    {
        _fakeUser.Role = role;

        // Adjust user properties based on role
        switch (role)
        {
            case UserRole.Manager:
                _fakeUser.EmployeeId = "MGR001";
                _fakeUser.UserName = "Jane Smith";
                _fakeUser.Email = "jane.smith@company.com";
                _fakeUser.Department = "Engineering";
                break;
            case UserRole.HR:
                _fakeUser.EmployeeId = "HR001";
                _fakeUser.UserName = "Alice Johnson";
                _fakeUser.Email = "alice.johnson@company.com";
                _fakeUser.Department = "Human Resources";
                break;
            default: // Employee
                _fakeUser.EmployeeId = "EMP001";
                _fakeUser.UserName = "Bob Brown";
                _fakeUser.Email = "bob.brown@company.com";
                _fakeUser.Department = "IT";
                break;
        }
    }
}