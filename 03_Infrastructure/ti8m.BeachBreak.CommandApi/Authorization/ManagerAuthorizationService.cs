using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.CommandApi.Authorization;

public class ManagerAuthorizationService : IManagerAuthorizationService
{
    private readonly UserContext userContext;
    private readonly IEmployeeRepository employeeRepository;
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;
    private readonly ILogger<ManagerAuthorizationService> logger;

    public ManagerAuthorizationService(
        UserContext userContext,
        IEmployeeRepository employeeRepository,
        IQuestionnaireAssignmentRepository assignmentRepository,
        ILogger<ManagerAuthorizationService> logger)
    {
        this.userContext = userContext;
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

    public async Task<bool> AreAllDirectReportsAsync(Guid managerId, IEnumerable<Guid> employeeIds)
    {
        try
        {
            var employeeIdList = employeeIds.ToList();
            if (!employeeIdList.Any())
            {
                return true; // Empty list is valid
            }

            // Get manager's direct reports
            var managerIdStr = managerId.ToString();
            var directReports = await employeeRepository.GetEmployeesByManagerIdAsync(managerIdStr);
            var directReportIds = directReports.Where(e => !e.IsDeleted).Select(e => e.Id).ToHashSet();

            // Check if all employee IDs are in the direct reports set
            var allAreDirectReports = employeeIdList.All(empId => directReportIds.Contains(empId));

            if (!allAreDirectReports)
            {
                var invalidIds = employeeIdList.Where(empId => !directReportIds.Contains(empId)).ToList();
                logger.LogWarning("Manager {ManagerId} attempted to access employees who are not direct reports: {InvalidIds}",
                    managerId, string.Join(", ", invalidIds));
            }

            return allAreDirectReports;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if employees {EmployeeIds} are direct reports of manager {ManagerId}",
                string.Join(", ", employeeIds), managerId);
            return false;
        }
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
