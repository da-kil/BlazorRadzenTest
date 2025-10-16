using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Core.Infrastructure.Authorization;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.QueryApi.Authorization;
using ti8m.BeachBreak.QueryApi.Controllers;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/assignments")]
[Authorize] // Base authorization - specific roles on individual endpoints
public class AssignmentsController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<AssignmentsController> logger;
    private readonly IManagerAuthorizationService authorizationService;
    private readonly IAuthorizationCacheService authorizationCacheService;

    public AssignmentsController(
        IQueryDispatcher queryDispatcher,
        ILogger<AssignmentsController> logger,
        IManagerAuthorizationService authorizationService,
        IAuthorizationCacheService authorizationCacheService)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
        this.authorizationService = authorizationService;
        this.authorizationCacheService = authorizationCacheService;
    }

    /// <summary>
    /// Gets all assignments. HR/Admin only.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "HR,HRLead,Admin")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAssignments()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new QuestionnaireAssignmentListQuery());
            return CreateResponse(result, templates =>
            {
                return templates.Select(template => MapToDto(template));
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments");
            return StatusCode(500, "An error occurred while retrieving assignments");
        }
    }

    /// <summary>
    /// Gets a specific assignment by ID.
    /// Managers can only view assignments for their direct reports.
    /// HR/Admin can view any assignment.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "TeamLead,HR,HRLead,Admin")]
    [ProducesResponseType(typeof(QuestionnaireAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssignment(Guid id)
    {
        try
        {
            // Get the assignment first
            var result = await queryDispatcher.QueryAsync(new QuestionnaireAssignmentQuery(id));
            if (result == null || !result.Succeeded || result.Payload == null)
                return NotFound($"Assignment with ID {id} not found");

            var assignment = result.Payload;

            // Check authorization - only apply manager restrictions if user doesn't have elevated HR/Admin roles
            Guid userId;
            try
            {
                userId = await authorizationService.GetCurrentManagerIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetAssignment authorization failed: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }

            var hasElevatedRole = await HasElevatedRoleAsync(userId);
            if (!hasElevatedRole)
            {
                var canAccess = await authorizationService.CanAccessAssignmentAsync(userId, id);

                if (!canAccess)
                {
                    logger.LogWarning("Manager {UserId} attempted to access assignment {AssignmentId} for non-direct report",
                        userId, id);
                    return Forbid();
                }
            }

            return CreateResponse(result, template => MapToDto(template));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignment {AssignmentId}", id);
            return StatusCode(500, "An error occurred while retrieving the assignment");
        }
    }

    /// <summary>
    /// Gets all assignments for a specific employee.
    /// Managers can only view assignments for their direct reports.
    /// HR/Admin can view assignments for any employee.
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    [Authorize(Roles = "TeamLead,HR,HRLead,Admin")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAssignmentsByEmployee(Guid employeeId)
    {
        try
        {
            // Check authorization - only apply manager restrictions if user doesn't have elevated HR/Admin roles
            Guid userId;
            try
            {
                userId = await authorizationService.GetCurrentManagerIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetAssignmentsByEmployee authorization failed: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }

            var hasElevatedRole = await HasElevatedRoleAsync(userId);
            if (!hasElevatedRole)
            {
                var isDirectReport = await authorizationService.IsManagerOfAsync(userId, employeeId);

                if (!isDirectReport)
                {
                    logger.LogWarning("Manager {UserId} attempted to access assignments for non-direct report employee {EmployeeId}",
                        userId, employeeId);
                    return Forbid();
                }
            }

            var result = await queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId));
            return CreateResponse(result, templates =>
            {
                return templates.Select(template => MapToDto(template));
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving assignments");
        }
    }

    /// <summary>
    /// Checks if the current user has an elevated role (HR, HRLead, or Admin).
    /// Returns true if elevated, false if user is only TeamLead/Employee.
    /// </summary>
    private async Task<bool> HasElevatedRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var employeeRole = await authorizationCacheService.GetEmployeeRoleCacheAsync<EmployeeRoleResult>(userId, cancellationToken);
        if (employeeRole == null)
        {
            logger.LogWarning("Unable to retrieve employee role for user {UserId}", userId);
            return false;
        }

        return employeeRole.ApplicationRole == ApplicationRole.HR ||
               employeeRole.ApplicationRole == ApplicationRole.HRLead ||
               employeeRole.ApplicationRole == ApplicationRole.Admin;
    }

    /// <summary>
    /// Maps a QuestionnaireAssignment query result to a QuestionnaireAssignmentDto.
    /// Includes all workflow properties for proper state management on the frontend.
    /// </summary>
    private static QuestionnaireAssignmentDto MapToDto(Application.Query.Queries.QuestionnaireAssignmentQueries.QuestionnaireAssignment assignment)
    {
        return new QuestionnaireAssignmentDto
        {
            AssignedBy = assignment.AssignedBy,
            AssignedDate = assignment.AssignedDate,
            CompletedDate = assignment.CompletedDate,
            DueDate = assignment.DueDate,
            EmployeeEmail = assignment.EmployeeEmail,
            EmployeeId = assignment.EmployeeId.ToString(),
            EmployeeName = assignment.EmployeeName,
            Id = assignment.Id,
            Notes = assignment.Notes,
            TemplateId = assignment.TemplateId,
            TemplateName = assignment.TemplateName,
            TemplateCategoryId = assignment.TemplateCategoryId,

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
            ManagerReviewFinishedDate = assignment.ManagerReviewFinishedDate,
            ManagerReviewFinishedBy = assignment.ManagerReviewFinishedBy,
            ManagerReviewSummary = assignment.ManagerReviewSummary,
            EmployeeReviewConfirmedDate = assignment.EmployeeReviewConfirmedDate,
            EmployeeReviewConfirmedBy = assignment.EmployeeReviewConfirmedBy,
            EmployeeReviewComments = assignment.EmployeeReviewComments,

            // Final state
            FinalizedDate = assignment.FinalizedDate,
            FinalizedBy = assignment.FinalizedBy,
            ManagerFinalNotes = assignment.ManagerFinalNotes,
            IsLocked = assignment.IsLocked
        };
    }

    /// <summary>
    /// Gets all review changes for a specific assignment.
    /// Returns a list of all edits made by the manager during the review meeting.
    /// </summary>
    [HttpGet("{id:guid}/review-changes")]
    [Authorize(Roles = "TeamLead,HR,HRLead,Admin")]
    [ProducesResponseType(typeof(IEnumerable<ReviewChangeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReviewChanges(Guid id)
    {
        try
        {
            // Check authorization - only apply manager restrictions if user doesn't have elevated HR/Admin roles
            Guid userId;
            try
            {
                userId = await authorizationService.GetCurrentManagerIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetReviewChanges authorization failed: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }

            var hasElevatedRole = await HasElevatedRoleAsync(userId);
            if (!hasElevatedRole)
            {
                var canAccess = await authorizationService.CanAccessAssignmentAsync(userId, id);

                if (!canAccess)
                {
                    logger.LogWarning("Manager {UserId} attempted to access review changes for assignment {AssignmentId} for non-direct report",
                        userId, id);
                    return Forbid();
                }
            }

            var result = await queryDispatcher.QueryAsync(new Application.Query.Queries.ReviewQueries.GetReviewChangesQuery(id));
            return CreateResponse(Result<List<Application.Query.Projections.ReviewChangeLogReadModel>>.Success(result), changes =>
            {
                return changes.Select(c => new ReviewChangeDto
                {
                    Id = c.Id,
                    AssignmentId = c.AssignmentId,
                    SectionId = c.SectionId,
                    SectionTitle = c.SectionTitle,
                    QuestionId = c.QuestionId,
                    QuestionTitle = c.QuestionTitle,
                    OriginalCompletionRole = c.OriginalCompletionRole,
                    OldValue = c.OldValue,
                    NewValue = c.NewValue,
                    ChangedAt = c.ChangedAt,
                    ChangedBy = c.ChangedBy
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving review changes for assignment {AssignmentId}", id);
            return StatusCode(500, "An error occurred while retrieving review changes");
        }
    }
}