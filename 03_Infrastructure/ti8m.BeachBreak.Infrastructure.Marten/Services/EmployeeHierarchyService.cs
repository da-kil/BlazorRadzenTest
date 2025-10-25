using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain.EmployeeAggregate.Services;

namespace ti8m.BeachBreak.Infrastructure.Marten.Services;

/// <summary>
/// Implementation of employee hierarchy service using Marten.
/// Handles team membership checks and hierarchical queries.
/// </summary>
public class EmployeeHierarchyService : IEmployeeHierarchyService
{
    private readonly IEmployeeAggregateRepository employeeRepository;
    private readonly ILogger<EmployeeHierarchyService> logger;

    public EmployeeHierarchyService(
        IEmployeeAggregateRepository employeeRepository,
        ILogger<EmployeeHierarchyService> logger)
    {
        this.employeeRepository = employeeRepository;
        this.logger = logger;
    }

    public async Task<bool> IsInTeamHierarchyAsync(
        Guid teamLeadId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        // Self-check: TeamLead is always in their own team
        if (teamLeadId == employeeId)
            return true;

        try
        {
            // Load the employee aggregate
            var employee = await employeeRepository.LoadAsync<Domain.EmployeeAggregate.Employee>(
                employeeId,
                cancellationToken: cancellationToken);

            if (employee == null)
            {
                logger.LogWarning("Employee {EmployeeId} not found during team hierarchy check", employeeId);
                return false;
            }

            // Walk up the manager hierarchy to see if we find the TeamLead
            var currentManagerId = employee.ManagerId;
            var visited = new HashSet<string>(); // Prevent infinite loops

            while (!string.IsNullOrEmpty(currentManagerId) && Guid.TryParse(currentManagerId, out var managerId))
            {
                // Check if this manager is the TeamLead
                if (managerId == teamLeadId)
                    return true;

                // Prevent cycles
                if (visited.Contains(currentManagerId))
                {
                    logger.LogWarning(
                        "Cycle detected in manager hierarchy for employee {EmployeeId} at manager {ManagerId}",
                        employeeId,
                        currentManagerId);
                    return false;
                }

                visited.Add(currentManagerId);

                // Load the manager to get their manager
                var manager = await employeeRepository.LoadAsync<Domain.EmployeeAggregate.Employee>(
                    managerId,
                    cancellationToken: cancellationToken);

                if (manager == null)
                    return false;

                currentManagerId = manager.ManagerId;

                // Safety limit: max 10 levels deep
                if (visited.Count > 10)
                {
                    logger.LogWarning(
                        "Manager hierarchy too deep for employee {EmployeeId}, stopping at 10 levels",
                        employeeId);
                    return false;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error checking team hierarchy for TeamLead {TeamLeadId} and Employee {EmployeeId}",
                teamLeadId,
                employeeId);
            return false; // Fail closed - deny access on error
        }
    }

    public async Task<bool> IsDirectManagerOfAsync(
        Guid managerId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var employee = await employeeRepository.LoadAsync<Domain.EmployeeAggregate.Employee>(
                employeeId,
                cancellationToken: cancellationToken);

            if (employee == null)
            {
                logger.LogWarning("Employee {EmployeeId} not found during direct manager check", employeeId);
                return false;
            }

            var isDirectManager = employee.ManagerId == managerId.ToString();

            if (!isDirectManager)
            {
                logger.LogDebug("Manager {ManagerId} is not the direct manager of employee {EmployeeId}",
                    managerId, employeeId);
            }

            return isDirectManager;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if {ManagerId} is direct manager of {EmployeeId}", managerId, employeeId);
            return false;
        }
    }

    public async Task<List<Guid>> GetTeamHierarchyIdsAsync(
        Guid teamLeadId,
        CancellationToken cancellationToken = default)
    {
        // NOTE: This method is kept for interface compatibility, but top-down hierarchy
        // traversal is more efficiently handled by the Query side (EmployeeVisibilityService)
        // which has optimized access to read models.
        //
        // For Command-side authorization checks, use IsInTeamHierarchyAsync() instead,
        // which performs bottom-up validation without loading entire hierarchies.
        //
        // If you need team hierarchy IDs for business logic, consider using the Query side
        // or refactor this to accept a read model repository for efficient traversal.

        logger.LogWarning(
            "GetTeamHierarchyIdsAsync is not fully implemented on the Domain side. " +
            "Use EmployeeVisibilityService (Query side) for efficient team hierarchy retrieval.");

        return await Task.FromResult(new List<Guid> { teamLeadId });
    }

    public async Task<List<Guid>> GetDirectReportIdsAsync(
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        // NOTE: Similar to GetTeamHierarchyIdsAsync, this is more efficiently handled
        // by the Query side (EmployeeVisibilityService) which has direct access to
        // read models and can use GetEmployeesByManagerIdAsync.
        //
        // The Domain service focuses on authorization checks (IsInTeamHierarchyAsync,
        // IsDirectManagerOfAsync) rather than bulk data retrieval.

        logger.LogWarning(
            "GetDirectReportIdsAsync is not fully implemented on the Domain side. " +
            "Use EmployeeVisibilityService (Query side) for efficient direct report retrieval.");

        return await Task.FromResult(new List<Guid>());
    }
}
