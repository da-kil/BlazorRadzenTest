using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;
using ti8m.BeachBreak.Application.Query.Queries.ProgressQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/managers")]
[Authorize(Roles = "TeamLead")]
public class ManagersController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<ManagersController> logger;
    private readonly UserContext userContext;

    public ManagersController(
        IQueryDispatcher queryDispatcher,
        ILogger<ManagersController> logger,
        UserContext userContext)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
        this.userContext = userContext;
    }

    /// <summary>
    /// Gets all team members (direct reports) for the authenticated manager.
    /// Uses UserContext to get the manager ID - more secure than accepting it as a parameter.
    /// </summary>
    [HttpGet("me/team")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyTeamMembers()
    {
        // Get manager ID from authenticated user context (security best practice)
        if (!Guid.TryParse(userContext.Id, out var managerId))
        {
            logger.LogWarning("GetMyTeamMembers failed: Unable to parse user ID from context");
            return Unauthorized("User ID not found in authentication context");
        }

        logger.LogInformation("Received GetMyTeamMembers request for authenticated ManagerId: {ManagerId}", managerId);

        try
        {
            var query = new GetTeamMembersQuery(managerId);
            var result = await queryDispatcher.QueryAsync(query);

            if (result.Succeeded)
            {
                var teamCount = result.Payload?.Count() ?? 0;
                logger.LogInformation("GetTeamMembers completed successfully for ManagerId: {ManagerId}, returned {TeamCount} members",
                    managerId, teamCount);
            }
            else
            {
                logger.LogWarning("GetTeamMembers failed for ManagerId: {ManagerId}, Error: {ErrorMessage}",
                    managerId, result.Message);
            }

            return CreateResponse(result, employees =>
            {
                return employees.Select(employee => new EmployeeDto
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Role = employee.Role,
                    EMail = employee.EMail,
                    StartDate = employee.StartDate,
                    EndDate = employee.EndDate,
                    LastStartDate = employee.LastStartDate,
                    ManagerId = employee.ManagerId,
                    Manager = employee.Manager,
                    LoginName = employee.LoginName,
                    EmployeeNumber = employee.EmployeeNumber,
                    OrganizationNumber = employee.OrganizationNumber,
                    Organization = employee.Organization,
                    IsDeleted = employee.IsDeleted,
                    ApplicationRole = employee.ApplicationRole
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving team members for manager {ManagerId}", managerId);
            return StatusCode(500, "An error occurred while retrieving team members");
        }
    }

    /// <summary>
    /// Gets all questionnaire assignments for the authenticated manager's team.
    /// Uses UserContext to get the manager ID - more secure than accepting it as a parameter.
    /// </summary>
    [HttpGet("me/assignments")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyTeamAssignments([FromQuery] string? status = null)
    {
        // Get manager ID from authenticated user context (security best practice)
        if (!Guid.TryParse(userContext.Id, out var managerId))
        {
            logger.LogWarning("GetMyTeamAssignments failed: Unable to parse user ID from context");
            return Unauthorized("User ID not found in authentication context");
        }

        logger.LogInformation("Received GetMyTeamAssignments request for authenticated ManagerId: {ManagerId}, Status: {Status}",
            managerId, status);

        try
        {
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus? filterStatus = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus>(status, true, out var parsedStatus))
            {
                filterStatus = parsedStatus;
            }

            var query = new GetTeamAssignmentsQuery(managerId, filterStatus);
            var result = await queryDispatcher.QueryAsync(query);

            if (result.Succeeded)
            {
                var assignmentCount = result.Payload?.Count() ?? 0;
                logger.LogInformation("GetTeamAssignments completed successfully for ManagerId: {ManagerId}, returned {AssignmentCount} assignments",
                    managerId, assignmentCount);
            }
            else
            {
                logger.LogWarning("GetTeamAssignments failed for ManagerId: {ManagerId}, Error: {ErrorMessage}",
                    managerId, result.Message);
            }

            return CreateResponse(result, assignments =>
            {
                return assignments.Select(assignment => new QuestionnaireAssignmentDto
                {
                    Id = assignment.Id,
                    EmployeeId = assignment.EmployeeId.ToString(),
                    EmployeeName = assignment.EmployeeName,
                    EmployeeEmail = assignment.EmployeeEmail,
                    TemplateId = assignment.TemplateId,
                    Status = MapAssignmentStatusToDto[assignment.Status],
                    AssignedDate = assignment.AssignedDate,
                    DueDate = assignment.DueDate,
                    CompletedDate = assignment.CompletedDate,
                    AssignedBy = assignment.AssignedBy,
                    Notes = assignment.Notes
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving team assignments for manager {ManagerId}", managerId);
            return StatusCode(500, "An error occurred while retrieving team assignments");
        }
    }

    /// <summary>
    /// Gets progress data for all assignments in the authenticated manager's team.
    /// Uses UserContext to get the manager ID - more secure than accepting it as a parameter.
    /// </summary>
    [HttpGet("me/team/progress")]
    [ProducesResponseType(typeof(IEnumerable<AssignmentProgressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyTeamProgress()
    {
        // Get manager ID from authenticated user context (security best practice)
        if (!Guid.TryParse(userContext.Id, out var managerId))
        {
            logger.LogWarning("GetMyTeamProgress failed: Unable to parse user ID from context");
            return Unauthorized("User ID not found in authentication context");
        }

        logger.LogInformation("Received GetMyTeamProgress request for authenticated ManagerId: {ManagerId}", managerId);

        try
        {
            var query = new GetTeamProgressQuery(managerId);
            var result = await queryDispatcher.QueryAsync(query);

            if (result.Succeeded)
            {
                var progressCount = result.Payload?.Count() ?? 0;
                logger.LogInformation("GetTeamProgress completed successfully for ManagerId: {ManagerId}, returned {ProgressCount} items",
                    managerId, progressCount);
            }
            else
            {
                logger.LogWarning("GetTeamProgress failed for ManagerId: {ManagerId}, Error: {ErrorMessage}",
                    managerId, result.Message);
            }

            return CreateResponse(result, progressItems =>
            {
                return progressItems.Select(progress => new AssignmentProgressDto
                {
                    AssignmentId = progress.AssignmentId,
                    ProgressPercentage = progress.ProgressPercentage,
                    TotalQuestions = progress.TotalQuestions,
                    AnsweredQuestions = progress.AnsweredQuestions,
                    LastModified = progress.LastModified,
                    IsCompleted = progress.IsCompleted,
                    TimeSpent = progress.TimeSpent
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving team progress for manager {ManagerId}", managerId);
            return StatusCode(500, "An error occurred while retrieving team progress");
        }
    }

    /// <summary>
    /// Gets analytics data for the authenticated manager's team.
    /// Uses UserContext to get the manager ID - more secure than accepting it as a parameter.
    /// </summary>
    [HttpGet("me/analytics")]
    [ProducesResponseType(typeof(TeamAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyTeamAnalytics()
    {
        // Get manager ID from authenticated user context (security best practice)
        if (!Guid.TryParse(userContext.Id, out var managerId))
        {
            logger.LogWarning("GetMyTeamAnalytics failed: Unable to parse user ID from context");
            return Unauthorized("User ID not found in authentication context");
        }

        logger.LogInformation("Received GetMyTeamAnalytics request for authenticated ManagerId: {ManagerId}", managerId);

        try
        {
            var query = new GetTeamAnalyticsQuery(managerId);
            var result = await queryDispatcher.QueryAsync(query);

            if (result.Succeeded)
            {
                logger.LogInformation("GetTeamAnalytics completed successfully for ManagerId: {ManagerId}", managerId);
            }
            else
            {
                logger.LogWarning("GetTeamAnalytics failed for ManagerId: {ManagerId}, Error: {ErrorMessage}",
                    managerId, result.Message);
            }

            return CreateResponse(result, analytics => new TeamAnalyticsDto
            {
                TotalTeamMembers = analytics.TotalTeamMembers,
                TotalAssignments = analytics.TotalAssignments,
                CompletedAssignments = analytics.CompletedAssignments,
                OverdueAssignments = analytics.OverdueAssignments,
                AverageCompletionTime = analytics.AverageCompletionTime,
                OnTimeCompletionRate = analytics.OnTimeCompletionRate,
                CategoryPerformance = analytics.CategoryPerformance.Select(cp => new CategoryPerformanceDto
                {
                    Category = cp.Category,
                    TotalAssignments = cp.TotalAssignments,
                    CompletedAssignments = cp.CompletedAssignments,
                    CompletionRate = cp.CompletionRate,
                    AverageCompletionTime = cp.AverageCompletionTime
                }).ToList(),
                EmployeePerformance = analytics.EmployeePerformance.Select(ep => new EmployeePerformanceDto
                {
                    EmployeeId = ep.EmployeeId,
                    EmployeeName = ep.EmployeeName,
                    TotalAssignments = ep.TotalAssignments,
                    CompletedAssignments = ep.CompletedAssignments,
                    OverdueAssignments = ep.OverdueAssignments,
                    CompletionRate = ep.CompletionRate,
                    AverageCompletionTime = ep.AverageCompletionTime
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving team analytics for manager {ManagerId}", managerId);
            return StatusCode(500, "An error occurred while retrieving team analytics");
        }
    }

    private static IReadOnlyDictionary<Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus, Dto.AssignmentStatus> MapAssignmentStatusToDto =>
        new Dictionary<Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus, Dto.AssignmentStatus>
        {
            { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Assigned, Dto.AssignmentStatus.Assigned },
            { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Overdue, Dto.AssignmentStatus.Overdue },
            { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Cancelled, Dto.AssignmentStatus.Cancelled },
            { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.InProgress, Dto.AssignmentStatus.InProgress },
            { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Completed, Dto.AssignmentStatus.Completed },
        };
}
