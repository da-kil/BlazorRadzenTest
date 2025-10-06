using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ti8m.BeachBreak.Client.Components.Shared;

/// <summary>
/// Base component that provides role-based authorization helpers for UI visibility.
/// Uses role hierarchy to determine access (e.g., Admin can access everything, HR can access HR+ features).
/// </summary>
public class AuthorizedComponentBase : ComponentBase
{
    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private AuthenticationState? _cachedAuthState;

    /// <summary>
    /// Gets the authentication state, using cached value if available.
    /// </summary>
    protected async Task<AuthenticationState?> GetAuthenticationStateAsync()
    {
        if (_cachedAuthState != null)
            return _cachedAuthState;

        if (AuthenticationStateTask == null)
            return null;

        _cachedAuthState = await AuthenticationStateTask;
        return _cachedAuthState;
    }

    /// <summary>
    /// Checks if user has the specified role or a higher role in the hierarchy.
    /// Note: This is synchronous for use in rendering. Call GetAuthenticationStateAsync() in OnInitializedAsync to cache.
    /// </summary>
    protected bool IsInRole(string role)
    {
        if (AuthenticationStateTask == null || !AuthenticationStateTask.IsCompleted)
            return false;

        var authState = AuthenticationStateTask.Result;
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated ?? true)
            return false;

        // Get user's role from claims (set by CustomAuthenticationStateProvider)
        var userRole = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userRole))
            return false;

        return CheckRoleAccess(userRole, role);
    }

    protected bool IsInAnyRole(params string[] roles)
    {
        return roles.Any(IsInRole);
    }

    // Convenient role check properties with hierarchy
    protected bool IsAdmin => IsInRole("Admin");
    protected bool IsHR => IsInRole("HR"); // HR, HRLead, Admin can access
    protected bool IsHRLead => IsInRole("HRLead"); // HRLead, Admin can access
    protected bool IsTeamLead => IsInRole("TeamLead"); // TeamLead, HR, HRLead, Admin can access
    protected bool IsEmployee
    {
        get
        {
            if (AuthenticationStateTask == null || !AuthenticationStateTask.IsCompleted)
                return false;
            return AuthenticationStateTask.Result.User.Identity?.IsAuthenticated ?? false;
        }
    }

    /// <summary>
    /// Checks if a user's role satisfies the required role based on hierarchy.
    /// Role hierarchy: Admin > HRLead > HR > TeamLead > Employee
    /// </summary>
    private static bool CheckRoleAccess(string userRole, string requiredRole)
    {
        // Define role hierarchy: which roles can access each policy
        var roleHierarchy = new Dictionary<string, string[]>
        {
            ["Employee"] = new[] { "Employee", "TeamLead", "HR", "HRLead", "Admin" },
            ["TeamLead"] = new[] { "TeamLead", "HR", "HRLead", "Admin" },
            ["HR"] = new[] { "HR", "HRLead", "Admin" },
            ["HRLead"] = new[] { "HRLead", "Admin" },
            ["Admin"] = new[] { "Admin" }
        };

        if (roleHierarchy.TryGetValue(requiredRole, out var allowedRoles))
        {
            return allowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase);
        }

        // If no hierarchy defined, do exact match
        return string.Equals(userRole, requiredRole, StringComparison.OrdinalIgnoreCase);
    }
}
