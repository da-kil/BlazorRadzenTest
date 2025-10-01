using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Services;

namespace ti8m.BeachBreak.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuthenticationService(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<CurrentUser> GetCurrentUserAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return new CurrentUser();
        }

        var role = user.FindFirst("ApplicationRole")?.Value
            ?? user.FindFirst(ClaimTypes.Role)?.Value
            ?? "Employee";

        return new CurrentUser
        {
            EmployeeId = user.FindFirst("EmployeeGuid")?.Value
                ?? user.FindFirst("EmployeeId")?.Value
                ?? string.Empty,
            Name = user.FindFirst("name")?.Value
                ?? user.FindFirst(ClaimTypes.Name)?.Value
                ?? string.Empty,
            Email = user.FindFirst("preferred_username")?.Value
                ?? user.FindFirst(ClaimTypes.Email)?.Value
                ?? string.Empty,
            Department = user.FindFirst("department")?.Value ?? string.Empty,
            Role = role,
            Title = user.FindFirst("jobTitle")?.Value ?? string.Empty,
            Permissions = [] // Permissions are now determined by authorization policies on the backend
        };
    }
}
