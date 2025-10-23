using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Service to determine which employees a user can see based on their ApplicationRole.
/// Implements the data filtering logic for the 5 user types.
/// Accepts userId (Guid) instead of ClaimsPrincipal to keep Application layer free from HTTP concerns.
/// </summary>
public class EmployeeVisibilityService(
    IEmployeeRepository employeeRepository,
    IQueryDispatcher queryDispatcher,
    ILogger<EmployeeVisibilityService> logger)
{
    /// <summary>
    /// Gets all employees visible to the current user based on their role.
    /// </summary>
    /// <param name="userId">The current user's ID (from UserContext)</param>
    public async Task<IEnumerable<EmployeeReadModel>> GetVisibleEmployeesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var role = await GetApplicationRoleAsync(userId, cancellationToken);

        return role switch
        {
            ApplicationRole.Admin => await GetAllEmployeesAsync(cancellationToken),
            ApplicationRole.HRLead => await GetAllEmployeesAsync(cancellationToken),
            ApplicationRole.HR => await GetAllEmployeesExceptHRAsync(cancellationToken),
            ApplicationRole.TeamLead => await GetTeamHierarchyAsync(userId, cancellationToken),
            ApplicationRole.Employee => await GetSelfOnlyAsync(userId, cancellationToken),
            _ => Enumerable.Empty<EmployeeReadModel>()
        };
    }

    /// <summary>
    /// Checks if the current user can view a specific employee.
    /// </summary>
    /// <param name="userId">The current user's ID (from UserContext)</param>
    public async Task<bool> CanViewEmployeeAsync(
        Guid userId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var visibleEmployees = await GetVisibleEmployeesAsync(userId, cancellationToken);
        return visibleEmployees.Any(e => e.Id == employeeId);
    }

    /// <summary>
    /// Gets the ApplicationRole from the database (matching authorization middleware approach).
    /// </summary>
    private async Task<ApplicationRole> GetApplicationRoleAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var roleResult = await queryDispatcher.QueryAsync(
                new GetEmployeeRoleByIdQuery(userId),
                cancellationToken);

            return roleResult?.ApplicationRole ?? ApplicationRole.Employee;
        }
        catch (Exception ex)
        {
            // LogWarning (not LogError) because:
            // 1. System gracefully degrades with a security-first fallback (Employee = most restrictive)
            // 2. User can still access their own data, no critical feature is broken
            // 3. Prevents alert fatigue while still capturing diagnostic information
            // Monitor: If this warning appears frequently, investigate database connectivity or role data integrity
            logger.LogWarning(ex,
                "Failed to retrieve role for user {UserId}. Defaulting to Employee role for security.",
                userId);
            return ApplicationRole.Employee; // Default to most restrictive on error
        }
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
        Guid userId,
        CancellationToken cancellationToken)
    {
        var allEmployees = await employeeRepository.GetEmployeesAsync(
            includeDeleted: false,
            cancellationToken: cancellationToken);

        // Build team hierarchy recursively
        var teamMembers = new HashSet<EmployeeReadModel>();
        var currentEmployee = allEmployees.FirstOrDefault(e => e.Id == userId);

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
        Guid userId,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetEmployeeByIdAsync(userId, cancellationToken);
        return employee != null
            ? new[] { employee }
            : Enumerable.Empty<EmployeeReadModel>();
    }
}
