using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
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
    private readonly IEmployeeRoleService employeeRoleService;
    private readonly UserContext userContext;

    public AssignmentsController(
        IQueryDispatcher queryDispatcher,
        ILogger<AssignmentsController> logger,
        IManagerAuthorizationService authorizationService,
        IEmployeeRoleService employeeRoleService,
        UserContext userContext)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
        this.authorizationService = authorizationService;
        this.employeeRoleService = employeeRoleService;
        this.userContext = userContext;
    }

    /// <summary>
    /// Gets all assignments. HR/Admin only.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "HR")]
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
    /// Note: Employees should use /employees/me/assignments/{id} instead.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "TeamLead")]
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

            // Get current user ID
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

            // Check if user has elevated role (HR/Admin) - they can access any assignment
            var hasElevatedRole = await HasElevatedRoleAsync(userId);
            if (!hasElevatedRole)
            {
                // Managers can only access assignments for their direct reports
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
    [Authorize(Policy = "TeamLead")]
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
        var employeeRole = await employeeRoleService.GetEmployeeRoleAsync(userId, cancellationToken);
        if (employeeRole == null)
        {
            logger.LogWarning("Unable to retrieve employee role for user {UserId}", userId);
            return false;
        }

        // EmployeeRoleResult.ApplicationRole is already Application.Query.ApplicationRole
        return employeeRole.ApplicationRole == Application.Query.Models.ApplicationRole.HR ||
               employeeRole.ApplicationRole == Application.Query.Models.ApplicationRole.HRLead ||
               employeeRole.ApplicationRole == Application.Query.Models.ApplicationRole.Admin;
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
            StartedDate = assignment.StartedDate,
            CompletedDate = assignment.CompletedDate,
            DueDate = assignment.DueDate,
            EmployeeEmail = assignment.EmployeeEmail,
            EmployeeId = assignment.EmployeeId.ToString(),
            EmployeeName = assignment.EmployeeName,
            Id = assignment.Id,
            Notes = assignment.Notes,
            TemplateId = assignment.TemplateId,
            RequiresManagerReview = assignment.RequiresManagerReview,
            TemplateName = assignment.TemplateName,
            TemplateCategoryId = assignment.TemplateCategoryId,

            // Withdrawal tracking
            IsWithdrawn = assignment.IsWithdrawn,
            WithdrawnDate = assignment.WithdrawnDate,
            WithdrawnByEmployeeId = assignment.WithdrawnByEmployeeId,
            WithdrawnByEmployeeName = assignment.WithdrawnByEmployeeName,
            WithdrawalReason = assignment.WithdrawalReason,

            // Workflow properties
            WorkflowState = assignment.WorkflowState,
            SectionProgress = assignment.SectionProgress,

            // Submission phase
            EmployeeSubmittedDate = assignment.EmployeeSubmittedDate,
            EmployeeSubmittedByEmployeeId = assignment.EmployeeSubmittedByEmployeeId,
            EmployeeSubmittedByEmployeeName = assignment.EmployeeSubmittedByEmployeeName,
            ManagerSubmittedDate = assignment.ManagerSubmittedDate,
            ManagerSubmittedByEmployeeId = assignment.ManagerSubmittedByEmployeeId,
            ManagerSubmittedByEmployeeName = assignment.ManagerSubmittedByEmployeeName,

            // Review phase
            ReviewInitiatedDate = assignment.ReviewInitiatedDate,
            ReviewInitiatedByEmployeeId = assignment.ReviewInitiatedByEmployeeId,
            ReviewInitiatedByEmployeeName = assignment.ReviewInitiatedByEmployeeName,
            ManagerReviewFinishedDate = assignment.ManagerReviewFinishedDate,
            ManagerReviewFinishedByEmployeeId = assignment.ManagerReviewFinishedByEmployeeId,
            ManagerReviewFinishedByEmployeeName = assignment.ManagerReviewFinishedByEmployeeName,
            ManagerReviewSummary = assignment.ManagerReviewSummary,
            EmployeeReviewConfirmedDate = assignment.EmployeeReviewConfirmedDate,
            EmployeeReviewConfirmedByEmployeeId = assignment.EmployeeReviewConfirmedByEmployeeId,
            EmployeeReviewConfirmedByEmployeeName = assignment.EmployeeReviewConfirmedByEmployeeName,
            EmployeeReviewComments = assignment.EmployeeReviewComments,

            // Final state
            FinalizedDate = assignment.FinalizedDate,
            FinalizedByEmployeeId = assignment.FinalizedByEmployeeId,
            FinalizedByEmployeeName = assignment.FinalizedByEmployeeName,
            ManagerFinalNotes = assignment.ManagerFinalNotes,
            IsLocked = assignment.IsLocked,

            // Reopen tracking (audit trail)
            LastReopenedDate = assignment.LastReopenedDate,
            LastReopenedByEmployeeId = assignment.LastReopenedByEmployeeId,
            LastReopenedByEmployeeName = assignment.LastReopenedByEmployeeName,
            LastReopenedByRole = assignment.LastReopenedByRole,
            LastReopenReason = assignment.LastReopenReason,

            // InReview notes system
            InReviewNotes = assignment.InReviewNotes.Select(note => new ti8m.BeachBreak.QueryApi.Dto.InReviewNoteDto
            {
                Id = note.Id,
                Content = note.Content,
                Timestamp = note.Timestamp,
                SectionId = note.SectionId,
                SectionTitle = note.SectionTitle,
                AuthorEmployeeId = note.AuthorEmployeeId,
                AuthorName = note.AuthorName
            }).ToList()
        };
    }

    /// <summary>
    /// Gets all review changes for a specific assignment.
    /// Returns a list of all edits made by the manager during the review meeting.
    /// </summary>
    [HttpGet("{id:guid}/review-changes")]
    [Authorize(Policy = "TeamLead")]
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

            // Batch fetch employee names for all changes
            var employeeIds = result.Select(c => c.ChangedByEmployeeId).Distinct().ToList();
            var employeeNames = new Dictionary<Guid, string>();

            foreach (var employeeId in employeeIds)
            {
                var employeeResult = await queryDispatcher.QueryAsync(new EmployeeQuery(employeeId));
                if (employeeResult?.Succeeded == true && employeeResult.Payload != null)
                {
                    var employee = employeeResult.Payload;
                    employeeNames[employeeId] = $"{employee.FirstName} {employee.LastName}";
                }
                else
                {
                    employeeNames[employeeId] = "Unknown";
                }
            }

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
                    ChangedBy = employeeNames.TryGetValue(c.ChangedByEmployeeId, out var name) ? name : "Unknown"
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving review changes for assignment {AssignmentId}", id);
            return StatusCode(500, "An error occurred while retrieving review changes");
        }
    }

    /// <summary>
    /// Gets all custom sections for a specific assignment.
    /// Custom sections are instance-specific and added during initialization phase.
    /// Managers can only view custom sections for their direct reports' assignments.
    /// HR/Admin can view custom sections for any assignment.
    /// </summary>
    [HttpGet("{assignmentId}/custom-sections")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(typeof(IEnumerable<QuestionSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomSections(Guid assignmentId)
    {
        try
        {
            // Get authenticated user ID
            if (!Guid.TryParse(userContext.Id, out var userId))
            {
                logger.LogWarning("Failed to parse user ID from context");
                return Unauthorized("User ID not found in authentication context");
            }

            // Check if user has elevated role (HR/Admin)
            var hasElevatedRole = await HasElevatedRoleAsync(userId);
            if (!hasElevatedRole)
            {
                // Managers can only access assignments for their direct reports
                var canAccess = await authorizationService.CanAccessAssignmentAsync(userId, assignmentId);
                if (!canAccess)
                {
                    logger.LogWarning("Manager {UserId} attempted to access custom sections for assignment {AssignmentId} for non-direct report",
                        userId, assignmentId);
                    return Forbid();
                }
            }

            var query = new GetAssignmentCustomSectionsQuery(assignmentId);
            var result = await queryDispatcher.QueryAsync(query, HttpContext.RequestAborted);

            // Map query model (strings) to DTOs (enums) - same pattern as templates
            return CreateResponse(result, sections => sections.Select(MapSectionToDto));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving custom sections for assignment {AssignmentId}", assignmentId);
            return StatusCode(500, "An error occurred while retrieving custom sections");
        }
    }

    #region Goal Queries

    /// <summary>
    /// Gets available predecessor questionnaires that can be linked for goal rating.
    /// Returns finalized questionnaires for same employee, same category, that have ANY goals.
    /// Managers can access this for their direct reports' assignments.
    /// </summary>
    [HttpGet("{assignmentId}/predecessors")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(typeof(IEnumerable<AvailablePredecessorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailablePredecessors(Guid assignmentId)
    {
        try
        {
            // Get authenticated user ID
            if (!Guid.TryParse(userContext.Id, out var userId))
            {
                logger.LogWarning("Failed to parse user ID from context");
                return Unauthorized("User ID not found in authentication context");
            }

            // Get the assignment first to determine the employee
            var assignmentResult = await queryDispatcher.QueryAsync(new QuestionnaireAssignmentQuery(assignmentId));
            if (assignmentResult == null || !assignmentResult.Succeeded || assignmentResult.Payload == null)
            {
                logger.LogWarning("Assignment {AssignmentId} not found", assignmentId);
                return NotFound($"Assignment with ID {assignmentId} not found");
            }

            var assignment = assignmentResult.Payload;
            var employeeId = assignment.EmployeeId;

            // Check if user has elevated role (HR/Admin) or is the employee themselves
            var hasElevatedRole = await HasElevatedRoleAsync(userId);
            if (!hasElevatedRole && userId != employeeId)
            {
                // Managers can only access assignments for their direct reports
                var canAccess = await authorizationService.CanAccessAssignmentAsync(userId, assignmentId);
                if (!canAccess)
                {
                    logger.LogWarning("Manager {UserId} attempted to access predecessors for assignment {AssignmentId} for non-direct report",
                        userId, assignmentId);
                    return Forbid();
                }
            }

            // Query uses the employee ID (not the requesting user ID) to get predecessors for the employee
            var query = new Application.Query.Queries.QuestionnaireAssignmentQueries.GetAvailablePredecessorsQuery(
                assignmentId, employeeId);

            var result = await queryDispatcher.QueryAsync(query, HttpContext.RequestAborted);

            if (result == null)
            {
                logger.LogWarning("Query returned null for assignment {AssignmentId}", assignmentId);
                return NotFound();
            }

            if (!result.Succeeded)
            {
                return CreateResponse(result);
            }

            return Ok(result.Payload);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting available predecessors for assignment {AssignmentId}", assignmentId);
            return StatusCode(500, "An error occurred while retrieving available predecessors");
        }
    }

    /// <summary>
    /// Gets all goal data for a specific question within an assignment.
    /// Includes goals added by Employee/Manager and ratings of predecessor goals.
    /// Goals are filtered based on workflow state and user role.
    /// </summary>
    [HttpGet("{assignmentId:guid}/goals/{questionId:guid}")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(typeof(GoalQuestionDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGoalQuestionData(Guid assignmentId, Guid questionId)
    {
        try
        {
            // Get current user ID
            if (!Guid.TryParse(userContext.Id, out var userId))
            {
                logger.LogWarning("Failed to parse user ID from context");
                return Unauthorized("User ID not found in authentication context");
            }

            // Determine current user's role using the employee role service
            var employeeRole = await employeeRoleService.GetEmployeeRoleAsync(userId, HttpContext.RequestAborted);
            if (employeeRole == null)
            {
                logger.LogWarning("Unable to retrieve employee role for user {UserId}", userId);
                return Unauthorized("Unable to determine user role");
            }

            // Use ApplicationRole directly (no premature mapping)
            // The query handler will determine proper role-based filtering
            var query = new Application.Query.Queries.QuestionnaireAssignmentQueries.GetGoalQuestionDataQuery(
                assignmentId, questionId, employeeRole.ApplicationRole);

            var result = await queryDispatcher.QueryAsync(query, HttpContext.RequestAborted);

            if (result == null)
            {
                logger.LogWarning("Query returned null for assignment {AssignmentId}, question {QuestionId}",
                    assignmentId, questionId);
                return NotFound();
            }

            if (!result.Succeeded)
            {
                return CreateResponse(result);
            }

            return Ok(result.Payload);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting goal data for assignment {AssignmentId}, question {QuestionId}",
                assignmentId, questionId);
            return StatusCode(500, "An error occurred while retrieving goal data");
        }
    }

    #endregion

    #region Section Mapping Helpers

    /// <summary>
    /// Maps query-side QuestionSection (strings) to DTO (enums) for client consumption.
    /// Same pattern used by QuestionnaireTemplatesController for consistency.
    /// </summary>
    private static QuestionSectionDto MapSectionToDto(Application.Query.Queries.QuestionnaireTemplateQueries.QuestionSection section)
    {
        return new QuestionSectionDto
        {
            Id = section.Id,
            TitleGerman = section.TitleGerman,
            TitleEnglish = section.TitleEnglish,
            DescriptionGerman = section.DescriptionGerman,
            DescriptionEnglish = section.DescriptionEnglish,
            Order = section.Order,
            CompletionRole = MapToCompletionRoleEnum(section.CompletionRole),
            Type = MapQuestionTypeFromString(section.Type),
            Configuration = section.Configuration,
            IsInstanceSpecific = section.IsInstanceSpecific
        };
    }

    private static Domain.QuestionnaireTemplateAggregate.CompletionRole MapToCompletionRoleEnum(string completionRole)
    {
        return completionRole?.ToLower() switch
        {
            "manager" => Domain.QuestionnaireTemplateAggregate.CompletionRole.Manager,
            "both" => Domain.QuestionnaireTemplateAggregate.CompletionRole.Both,
            _ => Domain.QuestionnaireTemplateAggregate.CompletionRole.Employee
        };
    }

    private static QueryApi.Dto.QuestionType MapQuestionTypeFromString(string type)
    {
        return type?.ToLower() switch
        {
            "textquestion" => QueryApi.Dto.QuestionType.TextQuestion,
            "goal" => QueryApi.Dto.QuestionType.Goal,
            "assessment" => QueryApi.Dto.QuestionType.Assessment,
            _ => QueryApi.Dto.QuestionType.Assessment
        };
    }

    #endregion
}