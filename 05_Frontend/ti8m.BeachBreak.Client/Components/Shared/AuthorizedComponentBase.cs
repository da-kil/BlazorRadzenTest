using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ti8m.BeachBreak.Client.Components.Shared;

/// <summary>
/// Base component that provides role-based authorization helpers for UI visibility.
/// </summary>
public class AuthorizedComponentBase : ComponentBase
{
    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    protected bool IsInRole(string role)
    {
        if (AuthenticationStateTask == null)
            return false;

        var authState = AuthenticationStateTask.Result;
        var user = authState.User;

        // Check ApplicationRole claim (set by middleware)
        var applicationRoleClaim = user.FindFirst("ApplicationRole")?.Value;
        if (!string.IsNullOrEmpty(applicationRoleClaim))
        {
            return CheckRoleAccess(applicationRoleClaim, role);
        }

        // Fallback to standard role claims (from Azure AD)
        return user.IsInRole(role);
    }

    protected bool IsInAnyRole(params string[] roles)
    {
        return roles.Any(IsInRole);
    }

    protected bool IsAdmin => IsInRole("Admin");
    protected bool IsHR => IsInAnyRole("HR", "HRLead", "Admin");
    protected bool IsHRLead => IsInAnyRole("HRLead", "Admin");
    protected bool IsTeamLead => IsInAnyRole("TeamLead", "HR", "HRLead", "Admin");
    protected bool IsEmployee => AuthenticationStateTask?.Result.User.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Checks if a user's ApplicationRole satisfies the required role based on hierarchy.
    /// </summary>
    private static bool CheckRoleAccess(string userRole, string requiredRole)
    {
        // Define role hierarchy mappings
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

        return string.Equals(userRole, requiredRole, StringComparison.OrdinalIgnoreCase);
    }
}
