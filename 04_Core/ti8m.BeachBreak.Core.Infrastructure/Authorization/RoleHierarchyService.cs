using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Core.Infrastructure.Authorization;

/// <summary>
/// Infrastructure service that delegates to domain ApplicationRoleAuthorizationService.
/// Provides convenience methods for authorization handlers.
/// </summary>
public static class RoleHierarchyService
{
    /// <summary>
    /// Policy to ApplicationRole mappings - delegated to domain service.
    /// </summary>
    public static Dictionary<string, ApplicationRole[]> PolicyRoleMappings =>
        ApplicationRoleAuthorizationService.PolicyRoleMappings;

    /// <summary>
    /// Determines which roles a user with the given role can assign - delegated to domain service.
    /// </summary>
    public static ApplicationRole[] GetAssignableRoles(ApplicationRole requesterRole) =>
        ApplicationRoleAuthorizationService.GetAssignableRoles(requesterRole);

    /// <summary>
    /// Checks if a user with the given role has access to the specified policy.
    /// </summary>
    public static bool HasAccessToPolicy(ApplicationRole userRole, string policyName)
    {
        if (PolicyRoleMappings.TryGetValue(policyName, out var allowedRoles))
        {
            return allowedRoles.Contains(userRole);
        }
        return false;
    }
}
