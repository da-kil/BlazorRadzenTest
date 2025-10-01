using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Transforms user claims by loading Employee data and adding ApplicationRole claim.
/// This runs automatically after authentication to enrich the ClaimsPrincipal.
/// </summary>
public class EmployeeClaimsTransformation(IEmployeeRepository employeeRepository) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Check if we've already transformed (avoid duplicate processing)
        if (principal.HasClaim(c => c.Type == "ApplicationRole"))
        {
            return principal;
        }

        // Get login name from token (try multiple claim types)
        var loginName = principal.FindFirst("preferred_username")?.Value
                       ?? principal.FindFirst(ClaimTypes.Name)?.Value
                       ?? principal.FindFirst(ClaimTypes.Upn)?.Value
                       ?? principal.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(loginName))
        {
            // No login name found - cannot load employee data
            return principal;
        }

        try
        {
            // Load employee from database
            var employee = await employeeRepository.GetEmployeeByLoginNameAsync(loginName);

            if (employee != null)
            {
                var identity = principal.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    // Add ApplicationRole as a role claim for [Authorize(Roles = "...")] to work
                    identity.AddClaim(new Claim(ClaimTypes.Role, employee.ApplicationRole.ToString()));

                    // Add custom claims for additional data
                    identity.AddClaim(new Claim("ApplicationRole", employee.ApplicationRole.ToString()));
                    identity.AddClaim(new Claim("EmployeeId", employee.EmployeeId));
                    identity.AddClaim(new Claim("EmployeeGuid", employee.Id.ToString()));
                    identity.AddClaim(new Claim("OrganizationNumber", employee.OrganizationNumber.ToString()));

                    // Add manager info if exists
                    if (!string.IsNullOrEmpty(employee.ManagerId))
                    {
                        identity.AddClaim(new Claim("ManagerId", employee.ManagerId));
                    }
                }
            }
        }
        catch
        {
            // If employee lookup fails, continue without adding claims
            // This allows the user to still be authenticated but with limited access
        }

        return principal;
    }
}
