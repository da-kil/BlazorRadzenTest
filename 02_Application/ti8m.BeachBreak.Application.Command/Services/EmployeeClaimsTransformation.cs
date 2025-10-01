using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Services;

/// <summary>
/// Transforms user claims by loading Employee aggregate and adding ApplicationRole claim.
/// This runs automatically after authentication to enrich the ClaimsPrincipal.
/// </summary>
public class EmployeeClaimsTransformation(
    IEmployeeAggregateRepository employeeRepository,
    ILogger<EmployeeClaimsTransformation> logger) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        logger.LogInformation("[CommandApi] EmployeeClaimsTransformation.TransformAsync called");

        // Check if we've already transformed (avoid duplicate processing)
        if (principal.HasClaim(c => c.Type == "ApplicationRole"))
        {
            logger.LogDebug("[CommandApi] Claims already transformed, skipping");
            return principal;
        }

        // Get login name from token (try multiple claim types)
        var loginName = principal.FindFirst("preferred_username")?.Value
                       ?? principal.FindFirst(ClaimTypes.Name)?.Value
                       ?? principal.FindFirst(ClaimTypes.Upn)?.Value
                       ?? principal.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(loginName))
        {
            logger.LogWarning("[CommandApi] No login name found in claims, cannot load employee data");
            return principal;
        }

        logger.LogInformation("[CommandApi] Loading employee data for LoginName: {LoginName}", loginName);

        try
        {
            // Load employee aggregate from event store
            var employee = await employeeRepository.GetByLoginNameAsync(loginName);

            if (employee != null)
            {
                logger.LogInformation("[CommandApi] Employee found: {EmployeeId}, Role: {Role}",
                    employee.EmployeeId, employee.ApplicationRole);

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

                    logger.LogInformation("[CommandApi] Successfully added {ClaimCount} claims for employee {EmployeeId}",
                        5 + (string.IsNullOrEmpty(employee.ManagerId) ? 0 : 1), employee.EmployeeId);
                }
            }
            else
            {
                logger.LogWarning("[CommandApi] Employee not found for LoginName: {LoginName}", loginName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[CommandApi] Error loading employee data for LoginName: {LoginName}", loginName);
            // If employee lookup fails, continue without adding claims
            // This allows the user to still be authenticated but with limited access
        }

        return principal;
    }
}
