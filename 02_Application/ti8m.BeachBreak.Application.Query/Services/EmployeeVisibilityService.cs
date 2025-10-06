using System.Security.Claims;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Service to determine which employees a user can see based on their ApplicationRole.
/// Implements the data filtering logic for the 5 user types.
/// Fetches ApplicationRole from database, not from claims.
/// </summary>
public class EmployeeVisibilityService(
    IEmployeeRepository employeeRepository,
    IQueryDispatcher queryDispatcher)
{
    /// <summary>
    /// Gets all employees visible to the current user based on their role.
    /// </summary>
    public async Task<IEnumerable<EmployeeReadModel>> GetVisibleEmployeesAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        var role = await GetApplicationRoleAsync(user, cancellationToken);

        return role switch
        {
            ApplicationRole.Admin => await GetAllEmployeesAsync(cancellationToken),
            ApplicationRole.HRLead => await GetAllEmployeesAsync(cancellationToken),
            ApplicationRole.HR => await GetAllEmployeesExceptHRAsync(cancellationToken),
            ApplicationRole.TeamLead => await GetTeamHierarchyAsync(user, cancellationToken),
            ApplicationRole.Employee => await GetSelfOnlyAsync(user, cancellationToken),
            _ => Enumerable.Empty<EmployeeReadModel>()
        };
    }

    /// <summary>
    /// Checks if the current user can view a specific employee.
    /// </summary>
    public async Task<bool> CanViewEmployeeAsync(
        ClaimsPrincipal user,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var visibleEmployees = await GetVisibleEmployeesAsync(user, cancellationToken);
        return visibleEmployees.Any(e => e.Id == employeeId);
    }

    /// <summary>
    /// Gets the ApplicationRole from the database (matching authorization middleware approach).
    /// </summary>
    private async Task<ApplicationRole> GetApplicationRoleAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (!userId.HasValue)
        {
            return ApplicationRole.Employee; // Default to most restrictive
        }

        try
        {
            var roleResult = await queryDispatcher.QueryAsync(
                new GetEmployeeRoleByIdQuery(userId.Value),
                cancellationToken);

            return roleResult?.ApplicationRole ?? ApplicationRole.Employee;
        }
        catch
        {
            return ApplicationRole.Employee; // Default to most restrictive on error
        }
    }

    /// <summary>
    /// Gets the user ID from claims (Entra ID object ID).
    /// </summary>
    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("oid")?.Value
                         ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                         ?? user.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Gets the current user's Employee ID from claims.
    /// </summary>
    private Guid? GetEmployeeId(ClaimsPrincipal user)
    {
        var employeeIdString = user.FindFirst("EmployeeId")?.Value
                             ?? user.FindFirst("EmployeeGuid")?.Value;

        return Guid.TryParse(employeeIdString, out var employeeId)
            ? employeeId
            : null;
    }

    /// <summary>
    /// Admin & HRLead: Get all employees
    /// </summary>
    private async Task<IEnumerable<EmployeeReadModel>> GetAllEmployeesAsync(
        CancellationToken cancellationToken)
    {
        return await employeeRepository.GetEmployeesAsync(
            includeDeleted: false,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// HR: Get all employees except other HR employees
    /// </summary>
    private async Task<IEnumerable<EmployeeReadModel>> GetAllEmployeesExceptHRAsync(
        CancellationToken cancellationToken)
    {
        var allEmployees = await employeeRepository.GetEmployeesAsync(
            includeDeleted: false,
            cancellationToken: cancellationToken);

        // Filter out employees with HR or HRLead roles
        return allEmployees.Where(e =>
            e.ApplicationRole != ApplicationRole.HR &&
            e.ApplicationRole != ApplicationRole.HRLead);
    }

    /// <summary>
    /// TeamLead: Get entire team hierarchy (all direct and indirect reports)
    /// </summary>
    private async Task<IEnumerable<EmployeeReadModel>> GetTeamHierarchyAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var currentEmployeeId = GetEmployeeId(user);
        if (!currentEmployeeId.HasValue)
        {
            return Enumerable.Empty<EmployeeReadModel>();
        }

        var allEmployees = await employeeRepository.GetEmployeesAsync(
            includeDeleted: false,
            cancellationToken: cancellationToken);

        // Build team hierarchy recursively
        var teamMembers = new HashSet<EmployeeReadModel>();
        var currentEmployee = allEmployees.FirstOrDefault(e => e.Id == currentEmployeeId.Value);

        if (currentEmployee != null)
        {
            // Add self
            teamMembers.Add(currentEmployee);

            // Add all direct and indirect reports
            AddTeamMembers(currentEmployee.Id.ToString(), allEmployees.ToList(), teamMembers);
        }

        return teamMembers;
    }

    /// <summary>
    /// Recursively adds team members to the set.
    /// </summary>
    private void AddTeamMembers(
        string managerId,
        List<EmployeeReadModel> allEmployees,
        HashSet<EmployeeReadModel> teamMembers)
    {
        var directReports = allEmployees.Where(e => e.ManagerId == managerId);

        foreach (var employee in directReports)
        {
            if (teamMembers.Add(employee)) // Only process if not already added (avoid cycles)
            {
                // Recursively add their reports
                AddTeamMembers(employee.Id.ToString(), allEmployees, teamMembers);
            }
        }
    }

    /// <summary>
    /// Employee: Get only self
    /// </summary>
    private async Task<IEnumerable<EmployeeReadModel>> GetSelfOnlyAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var employeeId = GetEmployeeId(user);
        if (!employeeId.HasValue)
        {
            return Enumerable.Empty<EmployeeReadModel>();
        }

        var employee = await employeeRepository.GetEmployeeByIdAsync(employeeId.Value, cancellationToken);
        return employee != null
            ? new[] { employee }
            : Enumerable.Empty<EmployeeReadModel>();
    }
}
