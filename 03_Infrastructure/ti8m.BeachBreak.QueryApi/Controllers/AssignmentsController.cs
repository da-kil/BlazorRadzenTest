using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.QueryApi.Authorization;
using ti8m.BeachBreak.QueryApi.Controllers;
using ti8m.BeachBreak.QueryApi.Dto;
using ti8m.BeachBreak.QueryApi.Mappers;

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
    private readonly IReviewChangeEnrichmentService reviewChangeEnrichmentService;

    public AssignmentsController(
        IQueryDispatcher queryDispatcher,
        ILogger<AssignmentsController> logger,
        IManagerAuthorizationService authorizationService,
        IEmployeeRoleService employeeRoleService,
        UserContext userContext,
        IReviewChangeEnrichmentService reviewChangeEnrichmentService)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
        this.authorizationService = authorizationService;
        this.employeeRoleService = employeeRoleService;
        this.userContext = userContext;
        this.reviewChangeEnrichmentService = reviewChangeEnrichmentService;
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
        return await ExecuteWithAuthorizationAsync(
            authorizationService,
            employeeRoleService,
            logger,
            async (managerId, hasElevatedRole) =>
            {
                var result = await queryDispatcher.QueryAsync(new QuestionnaireAssignmentQuery(id));
                if (result == null || !result.Succeeded || result.Payload == null)
                {
                    return Result<QuestionnaireAssignmentDto>.Fail(
                        $"Assignment with ID {id} not found",
                        StatusCodes.Status404NotFound);
                }

                return Result<QuestionnaireAssignmentDto>.Success(MapToDto(result.Payload));
            },
            resourceId: id,
            requiresResourceAccess: true);
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
        return await ExecuteWithAuthorizationAsync(
            authorizationService,
            employeeRoleService,
            logger,
            async (managerId, hasElevatedRole) =>
            {
                var result = await queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId));
                if (!result.Succeeded)
                    return Result<IEnumerable<QuestionnaireAssignmentDto>>.Fail(result.Message ?? "Failed to retrieve assignments", result.StatusCode);

                var dtos = result.Payload!.Select(template => MapToDto(template));
                return Result<IEnumerable<QuestionnaireAssignmentDto>>.Success(dtos);
            },
            resourceId: employeeId,
            requiresResourceAccess: true,
            resourceAccessCheck: authorizationService.IsManagerOfAsync);
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
            ProcessType = EnumConverter.MapToProcessType(assignment.ProcessType),
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
        return await ExecuteWithAuthorizationAsync(
            authorizationService,
            employeeRoleService,
            logger,
            async (managerId, hasElevatedRole) =>
            {
                var result = await queryDispatcher.QueryAsync(
                    new Application.Query.Queries.ReviewQueries.GetReviewChangesQuery(id));

                // Batch fetch employee names for all changes (efficient single query)
                var employeeIds = result.Select(c => c.ChangedByEmployeeId).Distinct();
                var employeeNames = await reviewChangeEnrichmentService.GetEmployeeNamesAsync(
                    employeeIds,
                    HttpContext.RequestAborted);

                // Map to DTOs with enriched employee names
                var dtos = result.Select(c => new ReviewChangeDto
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
                }).ToList();

                return Result<IEnumerable<ReviewChangeDto>>.Success(dtos);
            },
            resourceId: id,
            requiresResourceAccess: true);
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
            var hasElevatedRole = await HasElevatedRoleAsync(employeeRoleService, logger, userId);
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
            var hasElevatedRole = await HasElevatedRoleAsync(employeeRoleService, logger, userId);
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
            CompletionRole = EnumConverter.MapToCompletionRole(section.CompletionRole),
            Type = EnumConverter.MapToQuestionType(section.Type),
            Configuration = section.Configuration,
            IsInstanceSpecific = section.IsInstanceSpecific
        };
    }


    #endregion

    #region Employee Feedback Queries

    /// <summary>
    /// Gets all available employee feedback records that can be linked to this assignment.
    /// Returns non-deleted feedback for the employee.
    /// </summary>
    [HttpGet("{assignmentId}/feedback/available")]
    [Authorize]
    [ProducesResponseType(typeof(List<Application.Query.Projections.Models.LinkedEmployeeFeedbackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailableEmployeeFeedback(Guid assignmentId)
    {
        try
        {
            var query = new Application.Query.Queries.QuestionnaireAssignmentQueries.GetAvailableEmployeeFeedbackQuery(assignmentId);
            var result = await queryDispatcher.QueryAsync(query, HttpContext.RequestAborted);

            if (result == null)
            {
                logger.LogWarning("Query returned null for assignment {AssignmentId}", assignmentId);
                return NotFound();
            }

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting available feedback for assignment {AssignmentId}", assignmentId);
            return StatusCode(500, "An error occurred while retrieving available feedback");
        }
    }

    /// <summary>
    /// Gets all linked feedback data for a specific question within an assignment.
    /// Returns the full feedback details for all linked feedback records.
    /// </summary>
    [HttpGet("{assignmentId}/feedback/{questionId}")]
    [Authorize]
    [ProducesResponseType(typeof(Application.Query.Projections.Models.FeedbackQuestionDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFeedbackQuestionData(Guid assignmentId, Guid questionId)
    {
        try
        {
            var query = new Application.Query.Queries.QuestionnaireAssignmentQueries.GetFeedbackQuestionDataQuery(
                assignmentId, questionId);
            var result = await queryDispatcher.QueryAsync(query, HttpContext.RequestAborted);

            if (result == null)
            {
                logger.LogWarning("Query returned null for assignment {AssignmentId}, question {QuestionId}",
                    assignmentId, questionId);
                return NotFound();
            }

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting feedback data for assignment {AssignmentId}, question {QuestionId}",
                assignmentId, questionId);
            return StatusCode(500, "An error occurred while retrieving feedback data");
        }
    }

    #endregion
}