using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.QueryApi.Authorization;
using ti8m.BeachBreak.QueryApi.Dto;
using ti8m.BeachBreak.QueryApi.Mappers;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/employees")]
[Authorize] // All endpoints require authentication
public class EmployeesController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<EmployeesController> logger;
    private readonly UserContext userContext;
    private readonly IManagerAuthorizationService authorizationService;
    private readonly IEmployeeRoleService employeeRoleService;

    public EmployeesController(
        IQueryDispatcher queryDispatcher,
        ILogger<EmployeesController> logger,
        UserContext userContext,
        IManagerAuthorizationService authorizationService,
        IEmployeeRoleService employeeRoleService)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
        this.userContext = userContext;
        this.authorizationService = authorizationService;
        this.employeeRoleService = employeeRoleService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllEmployees(
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int? organizationNumber = null,
        [FromQuery] string? role = null,
        [FromQuery] Guid? managerId = null)
    {
        logger.LogInformation("Received GetAllEmployees request - IncludeDeleted: {IncludeDeleted}, OrganizationNumber: {OrganizationNumber}, Role: {Role}, ManagerId: {ManagerId}",
            includeDeleted, organizationNumber, role, managerId);

        try
        {
            // Get current user ID for authorization
            Guid userId;
            try
            {
                userId = await authorizationService.GetCurrentManagerIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetAllEmployees authorization failed: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }

            var hasElevatedRole = await HasElevatedRoleAsync(userId);

            // If user is TeamLead (not HR/Admin), restrict to their direct reports
            Guid? effectiveManagerId = managerId;
            if (!hasElevatedRole)
            {
                // TeamLead can only see their own team
                effectiveManagerId = userId;
                logger.LogInformation("TeamLead {UserId} requesting employees - restricting to their direct reports", userId);
            }

            var query = new EmployeeListQuery
            {
                IncludeDeleted = includeDeleted,
                OrganizationNumber = organizationNumber,
                Role = role,
                ManagerId = effectiveManagerId
            };

            var result = await queryDispatcher.QueryAsync(query);

            if (result.Succeeded && result.Payload != null)
            {
                var employeeCount = result.Payload.Count();
                logger.LogInformation("GetAllEmployees completed successfully, returned {EmployeeCount} employees", employeeCount);
            }
            else if (!result.Succeeded)
            {
                logger.LogWarning("GetAllEmployees failed: {ErrorMessage}", result.Message);
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
            logger.LogError(ex, "Error retrieving employees");
            return StatusCode(500, "An error occurred while retrieving employees");
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmployee(Guid id)
    {
        logger.LogInformation("Received GetEmployee request for EmployeeId: {EmployeeId}", id);

        try
        {
            // Get current user ID for authorization
            Guid userId;
            try
            {
                userId = await authorizationService.GetCurrentManagerIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetEmployee authorization failed: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }

            // Check authorization
            var hasElevatedRole = await HasElevatedRoleAsync(userId);
            var isSelf = userId == id;
            var isDirectReport = !isSelf && !hasElevatedRole && await authorizationService.IsManagerOfAsync(userId, id);

            // Allow if: viewing self, has elevated role, or is manager of this employee
            if (!isSelf && !hasElevatedRole && !isDirectReport)
            {
                logger.LogWarning("User {UserId} attempted to access employee {EmployeeId} without authorization", userId, id);
                return Forbid();
            }

            var result = await queryDispatcher.QueryAsync(new EmployeeQuery(id));

            if (result?.Payload == null)
            {
                logger.LogInformation("Employee not found for EmployeeId: {EmployeeId}", id);
                return NotFound($"Employee with ID {id} not found");
            }

            if (result.Succeeded)
            {
                logger.LogInformation("GetEmployee completed successfully for EmployeeId: {EmployeeId}", id);
            }
            else
            {
                logger.LogWarning("GetEmployee failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}", id, result.Message);
            }

            return CreateResponse(result, employee => new EmployeeDto
            {
                Id = id,
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
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving employee {EmployeeId}", id);
            return StatusCode(500, "An error occurred while retrieving the employee");
        }
    }

    /// <summary>
    /// Gets all questionnaire assignments for the currently authenticated employee.
    /// Uses UserContext to get the employee ID - more secure than accepting it as a parameter.
    /// Optionally filter by workflow state.
    /// </summary>
    [HttpGet("me/assignments")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyAssignments([FromQuery] string? workflowState = null)
    {
        // Get employee ID from authenticated user context (security best practice)
        if (!Guid.TryParse(userContext.Id, out var employeeId))
        {
            logger.LogWarning("GetMyAssignments failed: Unable to parse user ID from context");
            return Unauthorized("User ID not found in authentication context");
        }

        logger.LogInformation("Received GetMyAssignments request for authenticated EmployeeId: {EmployeeId}, WorkflowState: {WorkflowState}",
            employeeId, workflowState);

        try
        {
            var result = await queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId));

            if (result.Succeeded)
            {
                var assignmentCount = result.Payload?.Count() ?? 0;
                logger.LogInformation("GetMyAssignments completed successfully for EmployeeId: {EmployeeId}, returned {AssignmentCount} assignments",
                    employeeId, assignmentCount);
            }
            else
            {
                logger.LogWarning("GetMyAssignments failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}", employeeId, result.Message);
            }

            return CreateResponse(result, assignments =>
            {
                // Filter by workflow state if provided
                var filteredAssignments = assignments;
                if (!string.IsNullOrWhiteSpace(workflowState) &&
                    Enum.TryParse<Domain.QuestionnaireAssignmentAggregate.WorkflowState>(workflowState, true, out var parsedState))
                {
                    filteredAssignments = assignments.Where(a => a.WorkflowState == parsedState);
                }

                return filteredAssignments.Select(assignment => new QuestionnaireAssignmentDto
                {
                    Id = assignment.Id,
                    EmployeeId = assignment.EmployeeId.ToString(),
                    EmployeeName = assignment.EmployeeName,
                    EmployeeEmail = assignment.EmployeeEmail,
                    TemplateId = assignment.TemplateId,
                    ProcessType = MapProcessType(assignment.ProcessType),
                    TemplateName = assignment.TemplateName,
                    TemplateCategoryId = assignment.TemplateCategoryId,
                    WorkflowState = assignment.WorkflowState,
                    SectionProgress = assignment.SectionProgress,
                    AssignedDate = assignment.AssignedDate,
                    DueDate = assignment.DueDate,
                    StartedDate = assignment.StartedDate,
                    CompletedDate = assignment.CompletedDate,
                    AssignedBy = assignment.AssignedBy,
                    Notes = assignment.Notes,

                    // Withdrawal tracking
                    IsWithdrawn = assignment.IsWithdrawn,
                    WithdrawnDate = assignment.WithdrawnDate,
                    WithdrawnByEmployeeId = assignment.WithdrawnByEmployeeId,
                    WithdrawnByEmployeeName = assignment.WithdrawnByEmployeeName,
                    WithdrawalReason = assignment.WithdrawalReason,

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

                    // Reopen tracking
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
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments for authenticated employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving your assignments");
        }
    }

    /// <summary>
    /// Gets the dashboard metrics for the currently authenticated employee.
    /// Uses UserContext to get the employee ID - more secure than accepting it as a parameter.
    /// Returns assignment counts (pending, in progress, completed) and urgent assignments list.
    /// </summary>
    [HttpGet("me/dashboard")]
    [ProducesResponseType(typeof(EmployeeDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyDashboard()
    {
        // Get employee ID from authenticated user context (security best practice)
        if (!Guid.TryParse(userContext.Id, out var employeeId))
        {
            logger.LogWarning("GetMyDashboard failed: Unable to parse user ID from context");
            return Unauthorized("User ID not found in authentication context");
        }

        logger.LogInformation("Received GetMyDashboard request for authenticated EmployeeId: {EmployeeId}", employeeId);

        try
        {
            var result = await queryDispatcher.QueryAsync(new EmployeeDashboardQuery(employeeId));

            if (result?.Payload == null)
            {
                logger.LogInformation("Dashboard not found for EmployeeId: {EmployeeId} - this is expected for new employees with no assignments", employeeId);

                // Return empty dashboard for employees with no assignments yet
                return Ok(new EmployeeDashboardDto
                {
                    EmployeeId = employeeId,
                    EmployeeFullName = string.Empty,
                    EmployeeEmail = string.Empty,
                    PendingCount = 0,
                    InProgressCount = 0,
                    CompletedCount = 0,
                    UrgentAssignments = new List<UrgentAssignmentDto>(),
                    LastUpdated = DateTime.UtcNow
                });
            }

            if (result.Succeeded)
            {
                logger.LogInformation("GetMyDashboard completed successfully for EmployeeId: {EmployeeId}", employeeId);
            }
            else
            {
                logger.LogWarning("GetMyDashboard failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}", employeeId, result.Message);
            }

            return CreateResponse(result, dashboard => new EmployeeDashboardDto
            {
                EmployeeId = dashboard.EmployeeId,
                EmployeeFullName = dashboard.EmployeeFullName,
                EmployeeEmail = dashboard.EmployeeEmail,
                PendingCount = dashboard.PendingCount,
                InProgressCount = dashboard.InProgressCount,
                CompletedCount = dashboard.CompletedCount,
                UrgentAssignments = dashboard.UrgentAssignments.Select(ua => new UrgentAssignmentDto
                {
                    AssignmentId = ua.AssignmentId,
                    QuestionnaireTemplateName = ua.QuestionnaireTemplateName,
                    DueDate = ua.DueDate,
                    WorkflowState = ua.WorkflowState,
                    IsOverdue = ua.IsOverdue
                }).ToList(),
                LastUpdated = dashboard.LastUpdated
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving dashboard for authenticated employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving your dashboard");
        }
    }

    /// <summary>
    /// Gets a specific questionnaire assignment by ID for the currently authenticated employee.
    /// Uses UserContext to get the employee ID - more secure than accepting it as a parameter.
    /// </summary>
    [HttpGet("me/assignments/{assignmentId:guid}")]
    [ProducesResponseType(typeof(QuestionnaireAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyAssignmentById(Guid assignmentId)
    {
        // Get employee ID from authenticated user context (security best practice)
        if (!Guid.TryParse(userContext.Id, out var employeeId))
        {
            logger.LogWarning("GetMyAssignmentById failed: Unable to parse user ID from context");
            return Unauthorized("User ID not found in authentication context");
        }

        logger.LogInformation("Received GetMyAssignmentById request for authenticated EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
            employeeId, assignmentId);

        try
        {
            // Query all assignments for this employee
            var result = await queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId));

            if (!result.Succeeded)
            {
                logger.LogWarning("GetMyAssignmentById failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}",
                    employeeId, result.Message);
                return StatusCode(500, result.Message);
            }

            // Find the specific assignment
            var assignment = result.Payload?.FirstOrDefault(a => a.Id == assignmentId);

            if (assignment == null)
            {
                logger.LogWarning("Assignment {AssignmentId} not found for EmployeeId: {EmployeeId}", assignmentId, employeeId);
                return NotFound($"Assignment {assignmentId} not found or does not belong to you");
            }

            logger.LogInformation("GetMyAssignmentById completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                employeeId, assignmentId);

            // Map to DTO with complete workflow properties
            var dto = new QuestionnaireAssignmentDto
            {
                Id = assignment.Id,
                EmployeeId = assignment.EmployeeId.ToString(),
                EmployeeName = assignment.EmployeeName,
                EmployeeEmail = assignment.EmployeeEmail,
                TemplateId = assignment.TemplateId,
                ProcessType = MapProcessType(assignment.ProcessType),
                TemplateName = assignment.TemplateName,
                TemplateCategoryId = assignment.TemplateCategoryId,
                WorkflowState = assignment.WorkflowState,
                SectionProgress = assignment.SectionProgress,
                AssignedDate = assignment.AssignedDate,
                DueDate = assignment.DueDate,
                StartedDate = assignment.StartedDate,
                CompletedDate = assignment.CompletedDate,
                AssignedBy = assignment.AssignedBy,
                Notes = assignment.Notes,

                // Withdrawal tracking
                IsWithdrawn = assignment.IsWithdrawn,
                WithdrawnDate = assignment.WithdrawnDate,
                WithdrawnByEmployeeId = assignment.WithdrawnByEmployeeId,
                WithdrawnByEmployeeName = assignment.WithdrawnByEmployeeName,
                WithdrawalReason = assignment.WithdrawalReason,

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

                // Reopen tracking
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

            return Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignment {AssignmentId} for authenticated employee {EmployeeId}",
                assignmentId, employeeId);
            return StatusCode(500, "An error occurred while retrieving your assignment");
        }
    }

    /// <summary>
    /// Gets custom sections for a specific assignment for the currently authenticated employee.
    /// Returns instance-specific sections that were added during manager initialization.
    /// Employees can only access custom sections for their own assignments.
    /// </summary>
    [HttpGet("me/assignments/{assignmentId:guid}/custom-sections")]
    [ProducesResponseType(typeof(IEnumerable<QuestionSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyAssignmentCustomSections(Guid assignmentId)
    {
        // Get employee ID from authenticated user context
        if (!Guid.TryParse(userContext.Id, out var employeeId))
        {
            logger.LogWarning("GetMyAssignmentCustomSections failed: Unable to parse user ID from context");
            return Unauthorized("User ID not found in authentication context");
        }

        logger.LogInformation("Received GetMyAssignmentCustomSections request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
            employeeId, assignmentId);

        try
        {
            // First verify the assignment belongs to this employee
            var assignmentListResult = await queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId));

            if (!assignmentListResult.Succeeded)
            {
                logger.LogWarning("GetMyAssignmentCustomSections failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}",
                    employeeId, assignmentListResult.Message);
                return StatusCode(500, assignmentListResult.Message);
            }

            // Check if assignment exists and belongs to employee
            var assignment = assignmentListResult.Payload?.FirstOrDefault(a => a.Id == assignmentId);
            if (assignment == null)
            {
                logger.LogWarning("Assignment {AssignmentId} not found for EmployeeId: {EmployeeId}", assignmentId, employeeId);
                return NotFound($"Assignment {assignmentId} not found or does not belong to you");
            }

            // Get custom sections and map to DTOs with enums (same pattern as templates and manager endpoint)
            var customSectionsQuery = new GetAssignmentCustomSectionsQuery(assignmentId);
            var result = await queryDispatcher.QueryAsync(customSectionsQuery, HttpContext.RequestAborted);

            logger.LogInformation("GetMyAssignmentCustomSections completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                employeeId, assignmentId);

            return CreateResponse(result, sections => sections.Select(MapSectionToDto));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving custom sections for assignment {AssignmentId} for employee {EmployeeId}",
                assignmentId, employeeId);
            return StatusCode(500, "An error occurred while retrieving custom sections");
        }
    }

    /// <summary>
    /// Gets the questionnaire response for a specific assignment for the currently authenticated employee.
    /// Uses UserContext to get the employee ID - more secure than accepting it as a parameter.
    /// </summary>
    [HttpGet("me/responses/assignment/{assignmentId:guid}")]
    [ProducesResponseType(typeof(QuestionnaireResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyResponse(Guid assignmentId)
    {
        // Get employee ID from authenticated user context (security best practice)
        if (!Guid.TryParse(userContext.Id, out var employeeId))
        {
            logger.LogWarning("GetMyResponse failed: Unable to parse user ID from context");
            return CreateResponse(Result.Fail("User ID not found in authentication context", StatusCodes.Status401Unauthorized));
        }

        logger.LogInformation("Received GetMyResponse request for authenticated EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
            employeeId, assignmentId);

        try
        {
            var query = new Application.Query.Queries.ResponseQueries.GetResponseByAssignmentIdQuery(assignmentId);
            var response = await queryDispatcher.QueryAsync(query);

            if (response == null)
            {
                logger.LogInformation("Response not found for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);
                return CreateResponse(Result.Fail($"Response not found for assignment {assignmentId}", StatusCodes.Status404NotFound));
            }

            // Verify this response belongs to the requesting employee
            if (response.EmployeeId != employeeId)
            {
                logger.LogWarning("Employee {EmployeeId} attempted to access response for Assignment {AssignmentId} belonging to {ActualEmployeeId}",
                    employeeId, assignmentId, response.EmployeeId);
                return CreateResponse(Result.Fail("You do not have permission to access this response", StatusCodes.Status403Forbidden));
            }

            logger.LogInformation("GetMyResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                employeeId, assignmentId);

            // Map section responses
            // Note: Response structure is Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>>
            // For employee "me" endpoint, we only return EMPLOYEE responses
            var sectionResponsesDto = new Dictionary<Guid, SectionResponseDto>();
            foreach (var sectionKvp in response.SectionResponses)
            {
                var sectionId = sectionKvp.Key;
                var roleBasedResponses = sectionKvp.Value;

                var questionResponsesDict = new Dictionary<Guid, QuestionResponseDto>();

                // Only extract Employee role responses for this endpoint
                if (roleBasedResponses.TryGetValue(CompletionRole.Employee, out var employeeResponse))
                {
                    var questionResponseDto = new QuestionResponseDto
                    {
                        QuestionId = sectionId,
                        ResponseData = QuestionResponseMapper.MapToDto(employeeResponse),
                        QuestionType = QuestionResponseMapper.InferQuestionType(employeeResponse)
                    };

                    questionResponsesDict[sectionId] = questionResponseDto;
                }

                // Only include sections that have employee responses
                if (questionResponsesDict.Any())
                {
                    sectionResponsesDto[sectionId] = new SectionResponseDto
                    {
                        SectionId = sectionId,
                        RoleResponses = new Dictionary<ResponseRole, Dictionary<Guid, QuestionResponseDto>>
                        {
                            { ResponseRole.Employee, questionResponsesDict }
                        }
                    };
                }
            }

            var dto = new QuestionnaireResponseDto
            {
                Id = response.Id,
                AssignmentId = response.AssignmentId,
                TemplateId = response.TemplateId,
                EmployeeId = response.EmployeeId.ToString(),
                SectionResponses = sectionResponsesDto,
                StartedDate = response.StartedDate,
                ProgressPercentage = response.ProgressPercentage
            };

            return CreateResponse(Result<QuestionnaireResponseDto>.Success(dto));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving response for assignment {AssignmentId} and employee {EmployeeId}",
                assignmentId, employeeId);
            return CreateResponse(Result.Fail("An error occurred while retrieving your response", StatusCodes.Status500InternalServerError));
        }
    }

    /// <summary>
    /// Gets all goal data for a specific question within an assignment for the current employee.
    /// Includes goals added by Employee and ratings of predecessor goals.
    /// Goals are filtered based on workflow state - employees only see their own goals.
    /// </summary>
    [HttpGet("me/assignments/{assignmentId:guid}/goals/{questionId:guid}")]
    [ProducesResponseType(typeof(GoalQuestionDataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyGoalQuestionData(Guid assignmentId, Guid questionId)
    {
        try
        {
            // Get current user ID from UserContext
            if (!Guid.TryParse(userContext.Id, out var userId))
            {
                logger.LogWarning("Failed to parse user ID from context");
                return Unauthorized("User ID not found in authentication context");
            }

            // Verify the assignment belongs to the current employee
            var assignmentResult = await queryDispatcher.QueryAsync(
                new QuestionnaireEmployeeAssignmentListQuery(userId),
                HttpContext.RequestAborted);

            if (assignmentResult?.Payload == null || !assignmentResult.Payload.Any(a => a.Id == assignmentId))
            {
                logger.LogWarning("Employee {UserId} attempted to access assignment {AssignmentId} that doesn't belong to them",
                    userId, assignmentId);
                return NotFound($"Assignment {assignmentId} not found or does not belong to you");
            }

            // Employees always see goals with Employee role filter
            var query = new Application.Query.Queries.QuestionnaireAssignmentQueries.GetGoalQuestionDataQuery(
                assignmentId, questionId, ApplicationRoleMapper.MapFromDomain(Domain.EmployeeAggregate.ApplicationRole.Employee));

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

            logger.LogInformation("Returning goal data for employee {UserId}: Assignment {AssignmentId}, Question {QuestionId}, WorkflowState: {WorkflowState}, Goals Count: {GoalCount}",
                userId, assignmentId, questionId,
                result.Payload?.WorkflowState,
                result.Payload?.Goals?.Count ?? 0);

            if (result.Payload?.Goals != null)
            {
                foreach (var goal in result.Payload.Goals)
                {
                    logger.LogInformation("  Goal {GoalId}: AddedByRole={AddedByRole}", goal.Id, goal.AddedByRole);
                }
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

    /// <summary>
    /// Gets all questionnaire assignments for a specific employee by ID.
    /// Managers can only view assignments for their direct reports.
    /// HR/Admin can view assignments for any employee.
    /// </summary>
    [HttpGet("{employeeId:guid}/assignments")]
    [Authorize(Policy = "TeamLead")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmployeeAssignments(Guid employeeId)
    {
        logger.LogInformation("Received GetEmployeeAssignments request for EmployeeId: {EmployeeId}", employeeId);

        try
        {
            // Get current user ID for authorization
            Guid userId;
            try
            {
                userId = await authorizationService.GetCurrentManagerIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetEmployeeAssignments authorization failed: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }

            // Check authorization
            var hasElevatedRole = await HasElevatedRoleAsync(userId);
            if (!hasElevatedRole)
            {
                // TeamLead must be manager of this employee
                var isDirectReport = await authorizationService.IsManagerOfAsync(userId, employeeId);
                if (!isDirectReport)
                {
                    logger.LogWarning("Manager {UserId} attempted to access assignments for non-direct report employee {EmployeeId}",
                        userId, employeeId);
                    return Forbid();
                }
            }

            var result = await queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId));

            if (result.Succeeded)
            {
                var assignmentCount = result.Payload?.Count() ?? 0;
                logger.LogInformation("GetEmployeeAssignments completed successfully for EmployeeId: {EmployeeId}, returned {AssignmentCount} assignments",
                    employeeId, assignmentCount);
            }
            else
            {
                logger.LogWarning("GetEmployeeAssignments failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}", employeeId, result.Message);
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
                    WorkflowState = assignment.WorkflowState,
                    AssignedDate = assignment.AssignedDate,
                    DueDate = assignment.DueDate,
                    CompletedDate = assignment.CompletedDate,
                    AssignedBy = assignment.AssignedBy,
                    Notes = assignment.Notes,

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
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving employee assignments");
        }
    }

    /// <summary>
    /// Gets the preferred language for a specific employee.
    /// Users can only access their own language preference unless they have elevated roles.
    /// </summary>
    [HttpGet("{id:guid}/language")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmployeeLanguage(Guid id)
    {
        logger.LogInformation("Received GetEmployeeLanguage request for EmployeeId: {EmployeeId}", id);

        try
        {
            // Get current user ID for authorization
            Guid userId;
            try
            {
                userId = await authorizationService.GetCurrentManagerIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetEmployeeLanguage authorization failed: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }

            // Check authorization - users can only access their own language or have elevated role
            var hasElevatedRole = await HasElevatedRoleAsync(userId);
            var isSelf = userId == id;

            if (!isSelf && !hasElevatedRole)
            {
                logger.LogWarning("User {UserId} attempted to access language for employee {EmployeeId} without authorization", userId, id);
                return Forbid();
            }

            var result = await queryDispatcher.QueryAsync(new EmployeeQuery(id));

            if (result?.Payload == null)
            {
                logger.LogInformation("Employee not found for EmployeeId: {EmployeeId}", id);
                return NotFound($"Employee with ID {id} not found");
            }

            if (!result.Succeeded)
            {
                logger.LogWarning("GetEmployeeLanguage failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}", id, result.Message);
                return StatusCode(500, result.Message);
            }

            logger.LogInformation("GetEmployeeLanguage completed successfully for EmployeeId: {EmployeeId}, Language: {Language}",
                id, result.Payload.PreferredLanguage);

            // Return language as JSON string
            return Ok(result.Payload.PreferredLanguage.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving language for employee {EmployeeId}", id);
            return StatusCode(500, "An error occurred while retrieving the employee language");
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
    /// Maps query-side QuestionSection (strings) to DTO (enums) for client consumption.
    /// Same pattern used by QuestionnaireTemplatesController and AssignmentsController for consistency.
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

    private static CompletionRole MapToCompletionRoleEnum(string completionRole)
    {
        return completionRole?.ToLower() switch
        {
            "manager" => CompletionRole.Manager,
            "both" => CompletionRole.Both,
            _ => CompletionRole.Employee
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

    private static QueryApi.Dto.QuestionnaireProcessType MapProcessType(Core.Domain.QuestionnaireProcessType domainProcessType)
    {
        return domainProcessType switch
        {
            Core.Domain.QuestionnaireProcessType.PerformanceReview => QueryApi.Dto.QuestionnaireProcessType.PerformanceReview,
            Core.Domain.QuestionnaireProcessType.Survey => QueryApi.Dto.QuestionnaireProcessType.Survey,
            _ => throw new ArgumentOutOfRangeException(nameof(domainProcessType), domainProcessType, "Unknown process type")
        };
    }
}