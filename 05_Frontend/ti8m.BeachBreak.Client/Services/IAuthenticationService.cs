using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface IAuthenticationService
{
    Task<CurrentUser> GetCurrentUserAsync();
}

public class CurrentUser
{
    public string EmployeeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Employee, Manager, HR
    public string Title { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}