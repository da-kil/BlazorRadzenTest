using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Authorization;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.QueryApi.Mappers;

namespace ti8m.BeachBreak.QueryApi.Authorization;

public class ManagerAuthorizationService : IManagerAuthorizationService
{
    private readonly UserContext userContext;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IEmployeeRepository employeeRepository;
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;
    private readonly ILogger<ManagerAuthorizationService> logger;
    private readonly IAuthorizationCacheService authorizationCacheService;

    public ManagerAuthorizationService(
        UserContext userContext,
        IHttpContextAccessor httpContextAccessor,
        IEmployeeRepository employeeRepository,
        IQuestionnaireAssignmentRepository assignmentRepository,
        ILogger<ManagerAuthorizationService> logger,
        IAuthorizationCacheService authorizationCacheService)
    {
        this.userContext = userContext;
        this.httpContextAccessor = httpContextAccessor;
        this.employeeRepository = employeeRepository;
        this.assignmentRepository = assignmentRepository;
        this.logger = logger;
        this.authorizationCacheService = authorizationCacheService;
    }

    public Task<Guid> GetCurrentManagerIdAsync()
    {
        if (!Guid.TryParse(userContext.Id, out var managerId))
        {
            logger.LogWarning("Failed to get current manager ID: Unable to parse user ID from context");
            throw new UnauthorizedAccessException("User ID not found in authentication context");
        }

        return Task.FromResult(managerId);
    }

    public bool CanViewTeam(Guid requestingUserId, Guid targetManagerId)
    {
        // User can always view their own team
        if (requestingUserId == targetManagerId)
        {
            return true;
        }

        // Check if user has HR or Admin role using authorization cache
        var employeeRole = authorizationCacheService.GetEmployeeRoleCacheAsync<EmployeeRoleResult>(requestingUserId).GetAwaiter().GetResult();
        if (employeeRole == null)
        {
            logger.LogWarning("Unable to retrieve employee role for user {UserId} in CanViewTeam check", requestingUserId);
            return false;
        }

        // HR and HRLead can view any manager's team
        // employeeRole.ApplicationRole is already Application.Query.ApplicationRole - no conversion needed
        if (employeeRole.ApplicationRole == Application.Query.Models.ApplicationRole.HR ||
            employeeRole.ApplicationRole == Application.Query.Models.ApplicationRole.HRLead ||
            employeeRole.ApplicationRole == Application.Query.Models.ApplicationRole.Admin)
        {
            logger.LogInformation("User {RequestingUserId} granted access to view manager {TargetManagerId} team via elevated role",
                requestingUserId, targetManagerId);
            return true;
        }

        logger.LogWarning("User {RequestingUserId} denied access to view manager {TargetManagerId} team",
            requestingUserId, targetManagerId);
        return false;
    }

    public async Task<bool> IsManagerOfAsync(Guid managerId, Guid employeeId)
    {
        try
        {
            var employee = await employeeRepository.GetEmployeeByIdAsync(employeeId);

            if (employee == null)
            {
                logger.LogWarning("Employee {EmployeeId} not found for manager check", employeeId);
                return false;
            }

            var isManager = employee.ManagerId == managerId.ToString();

            if (!isManager)
            {
                logger.LogInformation("Manager {ManagerId} is not the manager of employee {EmployeeId}",
                    managerId, employeeId);
            }

            return isManager;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if {ManagerId} is manager of {EmployeeId}", managerId, employeeId);
            return false;
        }
    }

    public async Task<List<Guid>> GetDirectReportIdsAsync(Guid managerId)
    {
        try
        {
            var managerIdStr = managerId.ToString();
            var directReports = await employeeRepository.GetEmployeesByManagerIdAsync(managerIdStr);

            return directReports
                .Where(e => !e.IsDeleted)
                .Select(e => e.Id)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting direct reports for manager {ManagerId}", managerId);
            return new List<Guid>();
        }
    }

    public async Task<bool> CanAccessAssignmentAsync(Guid managerId, Guid assignmentId)
    {
        try
        {
            var assignment = await assignmentRepository.GetAssignmentByIdAsync(assignmentId);

            if (assignment == null)
            {
                logger.LogWarning("Assignment {AssignmentId} not found", assignmentId);
                return false;
            }

            // Check if the assignment belongs to one of the manager's direct reports
            var isDirectReport = await IsManagerOfAsync(managerId, assignment.EmployeeId);

            if (!isDirectReport)
            {
                logger.LogWarning("Manager {ManagerId} cannot access assignment {AssignmentId} - not their direct report",
                    managerId, assignmentId);
            }

            return isDirectReport;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking assignment access for manager {ManagerId}, assignment {AssignmentId}",
                managerId, assignmentId);
            return false;
        }
    }
}
