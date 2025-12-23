using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.EmployeeAggregate;

/// <summary>
/// Domain service that encapsulates authorization rules for changing employee application roles.
/// Business Rule: Users can only assign roles at their level or below.
/// - Admin can assign all roles
/// - HRLead can assign all roles except Admin
/// - HR can assign all roles except Admin and HRLead
/// </summary>
public static class ApplicationRoleAuthorizationService
{
    /// <summary>
    /// Validates if the requester can assign the specified role to an employee.
    /// </summary>
    /// <param name="requesterRole">The ApplicationRole of the user making the request</param>
    /// <param name="targetRole">The ApplicationRole to be assigned</param>
    /// <returns>DomainResult indicating success or failure with reason</returns>
    public static DomainResult CanAssignRole(ApplicationRole requesterRole, ApplicationRole targetRole)
    {
        var isAuthorized = requesterRole switch
        {
            ApplicationRole.Admin => true, // Admin can assign all roles
            ApplicationRole.HRLead => targetRole != ApplicationRole.Admin, // HRLead cannot assign Admin
            ApplicationRole.HR => targetRole != ApplicationRole.Admin && targetRole != ApplicationRole.HRLead, // HR cannot assign Admin or HRLead
            _ => false // Other roles cannot assign any roles
        };

        if (!isAuthorized)
        {
            return DomainResult.Failure(
                $"Users with {requesterRole} role cannot assign the {targetRole} role. " +
                $"You can only assign roles at your level or below.",
                403);
        }

        return DomainResult.Success();
    }

    /// <summary>
    /// Gets the list of roles that the requester is authorized to assign.
    /// </summary>
    /// <param name="requesterRole">The ApplicationRole of the user making the request</param>
    /// <returns>Array of ApplicationRoles that can be assigned</returns>
    public static ApplicationRole[] GetAssignableRoles(ApplicationRole requesterRole)
    {
        return requesterRole switch
        {
            ApplicationRole.Admin => new[]
            {
                ApplicationRole.Employee,
                ApplicationRole.TeamLead,
                ApplicationRole.HR,
                ApplicationRole.HRLead,
                ApplicationRole.Admin
            },
            ApplicationRole.HRLead => new[]
            {
                ApplicationRole.Employee,
                ApplicationRole.TeamLead,
                ApplicationRole.HR,
                ApplicationRole.HRLead
            },
            ApplicationRole.HR => new[]
            {
                ApplicationRole.Employee,
                ApplicationRole.TeamLead,
                ApplicationRole.HR
            },
            _ => Array.Empty<ApplicationRole>()
        };
    }

    /// <summary>
    /// Policy to ApplicationRole mappings for authorization checks.
    /// All roles inherit Employee's basic access and can access additional policy-protected endpoints.
    /// Policies ending with "OrApp" also allow service principals with DataSeeder app role (checked in ASP.NET Core policies).
    /// </summary>
    public static readonly Dictionary<string, ApplicationRole[]> PolicyRoleMappings = new()
    {
        ["Employee"] = [ApplicationRole.Employee, ApplicationRole.TeamLead, ApplicationRole.HR, ApplicationRole.HRLead, ApplicationRole.Admin],
        ["Admin"] = [ApplicationRole.Admin],
        ["AdminOrApp"] = [ApplicationRole.Admin], // Same as Admin, plus service principals with DataSeeder role (checked in ASP.NET Core policy)
        ["HR"] = [ApplicationRole.HR, ApplicationRole.HRLead, ApplicationRole.Admin],
        ["HROrApp"] = [ApplicationRole.HR, ApplicationRole.HRLead, ApplicationRole.Admin], // Same as HR, plus service principals with DataSeeder role (checked in ASP.NET Core policy)
        ["HRLead"] = [ApplicationRole.HRLead, ApplicationRole.Admin],
        ["TeamLead"] = [ApplicationRole.TeamLead, ApplicationRole.HR, ApplicationRole.HRLead, ApplicationRole.Admin],
        ["TeamLeadOrApp"] = [ApplicationRole.TeamLead, ApplicationRole.HR, ApplicationRole.HRLead, ApplicationRole.Admin]
    };
}
