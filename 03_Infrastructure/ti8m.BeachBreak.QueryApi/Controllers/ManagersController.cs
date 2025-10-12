using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;
using ti8m.BeachBreak.QueryApi.Authorization;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/managers")]
[Authorize(Roles = "TeamLead")]
public class ManagersController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<ManagersController> logger;
    private readonly IManagerAuthorizationService authorizationService;

    public ManagersController(
        IQueryDispatcher queryDispatcher,
        ILogger<ManagersController> logger,
        IManagerAuthorizationService authorizationService)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
        this.authorizationService = authorizationService;
    }

    /// <summary>
    /// Gets all team members (direct reports) for the authenticated manager.
    /// Uses authorization service to get the manager ID securely.
    /// </summary>
    [HttpGet("me/team")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyTeamMembers()
    {
        Guid managerId;
        try
        {
            managerId = await authorizationService.GetCurrentManagerIdAsync();
            logger.LogInformation("Received GetMyTeamMembers request for authenticated ManagerId: {ManagerId}", managerId);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("GetMyTeamMembers failed: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }

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
    /// Uses authorization service to get the manager ID securely.
    /// Returns enriched data including template names.
    /// </summary>
    [HttpGet("me/assignments")]
    [ProducesResponseType(typeof(IEnumerable<TeamAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyTeamAssignments([FromQuery] string? status = null)
    {
        Guid managerId;
        try
        {
            managerId = await authorizationService.GetCurrentManagerIdAsync();
            logger.LogInformation("Received GetMyTeamAssignments request for authenticated ManagerId: {ManagerId}, Status: {Status}",
                managerId, status);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("GetMyTeamAssignments failed: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }

        try
        {
            WorkflowState? filterWorkflowState = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<WorkflowState>(status, true, out var parsedState))
            {
                filterWorkflowState = parsedState;
            }

            var query = new GetTeamAssignmentsQuery(managerId, filterWorkflowState);
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
                return assignments.Select(assignment => new TeamAssignmentDto
                {
                    Id = assignment.Id,
                    EmployeeId = assignment.EmployeeId.ToString(),
                    EmployeeName = assignment.EmployeeName,
                    EmployeeEmail = assignment.EmployeeEmail,
                    TemplateId = assignment.TemplateId,
                    TemplateName = assignment.TemplateName,
                    TemplateCategoryId = assignment.TemplateCategoryId,
                    AssignedDate = assignment.AssignedDate,
                    DueDate = assignment.DueDate,
                    CompletedDate = assignment.CompletedDate,
                    AssignedBy = assignment.AssignedBy,
                    Notes = assignment.Notes,

                    // Workflow properties
                    WorkflowState = assignment.WorkflowState,
                    SectionProgress = assignment.SectionProgress,

                    // Submission phase
                    EmployeeSubmittedDate = assignment.EmployeeSubmittedDate,
                    EmployeeSubmittedBy = assignment.EmployeeSubmittedBy,
                    ManagerSubmittedDate = assignment.ManagerSubmittedDate,
                    ManagerSubmittedBy = assignment.ManagerSubmittedBy,

                    // Review phase
                    ReviewInitiatedDate = assignment.ReviewInitiatedDate,
                    ReviewInitiatedBy = assignment.ReviewInitiatedBy,
                    EmployeeReviewConfirmedDate = assignment.EmployeeReviewConfirmedDate,
                    EmployeeReviewConfirmedBy = assignment.EmployeeReviewConfirmedBy,
                    ManagerReviewConfirmedDate = assignment.ManagerReviewConfirmedDate,
                    ManagerReviewConfirmedBy = assignment.ManagerReviewConfirmedBy,

                    // Final state
                    FinalizedDate = assignment.FinalizedDate,
                    FinalizedBy = assignment.FinalizedBy,
                    IsLocked = assignment.IsLocked
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
    /// Uses authorization service to get the manager ID securely.
    /// </summary>
    [HttpGet("me/team/progress")]
    [ProducesResponseType(typeof(IEnumerable<AssignmentProgressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyTeamProgress()
    {
        Guid managerId;
        try
        {
            managerId = await authorizationService.GetCurrentManagerIdAsync();
            logger.LogInformation("Received GetMyTeamProgress request for authenticated ManagerId: {ManagerId}", managerId);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("GetMyTeamProgress failed: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }

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
    /// Uses authorization service to get the manager ID securely.
    /// </summary>
    [HttpGet("me/analytics")]
    [ProducesResponseType(typeof(TeamAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyTeamAnalytics()
    {
        Guid managerId;
        try
        {
            managerId = await authorizationService.GetCurrentManagerIdAsync();
            logger.LogInformation("Received GetMyTeamAnalytics request for authenticated ManagerId: {ManagerId}", managerId);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("GetMyTeamAnalytics failed: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }

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

    /// <summary>
    /// Gets all team members for a specific manager. HR/Admin only.
    /// </summary>
    [HttpGet("{managerId:guid}/team")]
    [Authorize(Roles = "HR,HRLead,Admin")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetManagerTeamMembers(Guid managerId)
    {
        Guid requestingUserId;
        try
        {
            requestingUserId = await authorizationService.GetCurrentManagerIdAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("GetManagerTeamMembers failed: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }

        // Check authorization
        if (!authorizationService.CanViewTeam(requestingUserId, managerId))
        {
            logger.LogWarning("User {RequestingUserId} not authorized to view manager {ManagerId} team",
                requestingUserId, managerId);
            return Forbid();
        }

        logger.LogInformation("User {RequestingUserId} viewing team for ManagerId: {ManagerId}",
            requestingUserId, managerId);

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
    /// Gets all questionnaire assignments for a specific manager's team. HR/Admin only.
    /// Returns enriched data including template names.
    /// </summary>
    [HttpGet("{managerId:guid}/assignments")]
    [Authorize(Roles = "HR,HRLead,Admin")]
    [ProducesResponseType(typeof(IEnumerable<TeamAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetManagerTeamAssignments(Guid managerId, [FromQuery] string? status = null)
    {
        Guid requestingUserId;
        try
        {
            requestingUserId = await authorizationService.GetCurrentManagerIdAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("GetManagerTeamAssignments failed: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }

        // Check authorization
        if (!authorizationService.CanViewTeam(requestingUserId, managerId))
        {
            logger.LogWarning("User {RequestingUserId} not authorized to view manager {ManagerId} team assignments",
                requestingUserId, managerId);
            return Forbid();
        }

        logger.LogInformation("User {RequestingUserId} viewing team assignments for ManagerId: {ManagerId}, Status: {Status}",
            requestingUserId, managerId, status);

        try
        {
            WorkflowState? filterWorkflowState = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<WorkflowState>(status, true, out var parsedState))
            {
                filterWorkflowState = parsedState;
            }

            var query = new GetTeamAssignmentsQuery(managerId, filterWorkflowState);
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
                return assignments.Select(assignment => new TeamAssignmentDto
                {
                    Id = assignment.Id,
                    EmployeeId = assignment.EmployeeId.ToString(),
                    EmployeeName = assignment.EmployeeName,
                    EmployeeEmail = assignment.EmployeeEmail,
                    TemplateId = assignment.TemplateId,
                    TemplateName = assignment.TemplateName,
                    TemplateCategoryId = assignment.TemplateCategoryId,
                    AssignedDate = assignment.AssignedDate,
                    DueDate = assignment.DueDate,
                    CompletedDate = assignment.CompletedDate,
                    AssignedBy = assignment.AssignedBy,
                    Notes = assignment.Notes,

                    // Workflow properties
                    WorkflowState = assignment.WorkflowState,
                    SectionProgress = assignment.SectionProgress,

                    // Submission phase
                    EmployeeSubmittedDate = assignment.EmployeeSubmittedDate,
                    EmployeeSubmittedBy = assignment.EmployeeSubmittedBy,
                    ManagerSubmittedDate = assignment.ManagerSubmittedDate,
                    ManagerSubmittedBy = assignment.ManagerSubmittedBy,

                    // Review phase
                    ReviewInitiatedDate = assignment.ReviewInitiatedDate,
                    ReviewInitiatedBy = assignment.ReviewInitiatedBy,
                    EmployeeReviewConfirmedDate = assignment.EmployeeReviewConfirmedDate,
                    EmployeeReviewConfirmedBy = assignment.EmployeeReviewConfirmedBy,
                    ManagerReviewConfirmedDate = assignment.ManagerReviewConfirmedDate,
                    ManagerReviewConfirmedBy = assignment.ManagerReviewConfirmedBy,

                    // Final state
                    FinalizedDate = assignment.FinalizedDate,
                    FinalizedBy = assignment.FinalizedBy,
                    IsLocked = assignment.IsLocked
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving team assignments for manager {ManagerId}", managerId);
            return StatusCode(500, "An error occurred while retrieving team assignments");
        }
    }

}
