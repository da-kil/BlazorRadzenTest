using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.QueryApi.Authorization;

public class ManagerAuthorizationService : IManagerAuthorizationService
{
    private readonly UserContext userContext;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IEmployeeRepository employeeRepository;
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;
    private readonly ILogger<ManagerAuthorizationService> logger;

    public ManagerAuthorizationService(
        UserContext userContext,
        IHttpContextAccessor httpContextAccessor,
        IEmployeeRepository employeeRepository,
        IQuestionnaireAssignmentRepository assignmentRepository,
        ILogger<ManagerAuthorizationService> logger)
    {
        this.userContext = userContext;
        this.httpContextAccessor = httpContextAccessor;
        this.employeeRepository = employeeRepository;
        this.assignmentRepository = assignmentRepository;
        this.logger = logger;
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

        // Check if user has HR or Admin role
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            logger.LogWarning("No user context available for CanViewTeam check");
            return false;
        }

        // HR and HRLead can view any manager's team
        if (user.IsInRole("HR") || user.IsInRole("HRLead") || user.IsInRole("Admin"))
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
