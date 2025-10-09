using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Core.Infrastructure.Authorization;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.QueryApi.Authorization;
using ti8m.BeachBreak.QueryApi.Dto;

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
    private readonly IAuthorizationCacheService authorizationCacheService;

    public EmployeesController(
        IQueryDispatcher queryDispatcher,
        ILogger<EmployeesController> logger,
        UserContext userContext,
        IManagerAuthorizationService authorizationService,
        IAuthorizationCacheService authorizationCacheService)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
        this.userContext = userContext;
        this.authorizationService = authorizationService;
        this.authorizationCacheService = authorizationCacheService;
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
    /// </summary>
    [HttpGet("me/assignments")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyAssignments()
    {
        // Get employee ID from authenticated user context (security best practice)
        if (!Guid.TryParse(userContext.Id, out var employeeId))
        {
            logger.LogWarning("GetMyAssignments failed: Unable to parse user ID from context");
            return Unauthorized("User ID not found in authentication context");
        }

        logger.LogInformation("Received GetMyAssignments request for authenticated EmployeeId: {EmployeeId}", employeeId);

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
            logger.LogError(ex, "Error retrieving assignments for authenticated employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving your assignments");
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

            // Map to DTO (same as GetMyAssignments)
            var dto = new QuestionnaireAssignmentDto
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

            // Map section responses - deserialize from Marten's JSON storage
            // Note: Response structure is Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, object>>>
            // For employee "me" endpoint, we only return EMPLOYEE responses
            var sectionResponsesDto = new Dictionary<Guid, SectionResponseDto>();
            foreach (var sectionKvp in response.SectionResponses)
            {
                var sectionId = sectionKvp.Key;
                Dictionary<Domain.QuestionnaireTemplateAggregate.CompletionRole, Dictionary<Guid, object>>? roleBasedResponses = null;

                // Handle the fact that sectionKvp.Value might be a JsonElement or already the correct type
                if (sectionKvp.Value is System.Text.Json.JsonElement roleJsonElement)
                {
                    roleBasedResponses = System.Text.Json.JsonSerializer.Deserialize<Dictionary<Domain.QuestionnaireTemplateAggregate.CompletionRole, Dictionary<Guid, object>>>(roleJsonElement.GetRawText());
                }
                else if (sectionKvp.Value is Dictionary<Domain.QuestionnaireTemplateAggregate.CompletionRole, Dictionary<Guid, object>> typedRoleResponses)
                {
                    roleBasedResponses = typedRoleResponses;
                }

                if (roleBasedResponses == null) continue;

                var questionResponsesDict = new Dictionary<Guid, QuestionResponseDto>();

                // Only extract Employee role responses for this endpoint
                if (roleBasedResponses.TryGetValue(Domain.QuestionnaireTemplateAggregate.CompletionRole.Employee, out var employeeResponses))
                {
                    // employeeResponses is Dictionary<Guid, object> where each value might be JsonElement
                    foreach (var questionKvp in employeeResponses)
                    {
                        if (questionKvp.Value is System.Text.Json.JsonElement qJsonElement)
                        {
                            var questionResponse = System.Text.Json.JsonSerializer.Deserialize<QuestionResponseDto>(qJsonElement.GetRawText());
                            if (questionResponse != null)
                            {
                                questionResponsesDict[questionKvp.Key] = questionResponse;
                            }
                        }
                        else
                        {
                            // Fallback: create a simple response with just the value
                            questionResponsesDict[questionKvp.Key] = new QuestionResponseDto
                            {
                                QuestionId = questionKvp.Key,
                                Value = questionKvp.Value
                            };
                        }
                    }
                }

                // Only include sections that have employee responses
                if (questionResponsesDict.Any())
                {
                    sectionResponsesDto[sectionId] = new SectionResponseDto
                    {
                        SectionId = sectionId,
                        QuestionResponses = questionResponsesDict
                    };
                }
            }

            var dto = new QuestionnaireResponseDto
            {
                Id = response.Id,
                AssignmentId = response.AssignmentId,
                TemplateId = response.TemplateId,
                EmployeeId = response.EmployeeId.ToString(),
                Status = MapResponseStatusToDto(response.Status),
                SectionResponses = sectionResponsesDto,
                StartedDate = response.StartedDate,
                CompletedDate = response.SubmittedDate,
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
    /// Gets all questionnaire assignments for a specific employee by ID.
    /// Managers can only view assignments for their direct reports.
    /// HR/Admin can view assignments for any employee.
    /// </summary>
    [HttpGet("{employeeId:guid}/assignments")]
    [Authorize(Roles = "TeamLead,HR,HRLead,Admin")]
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
            logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving employee assignments");
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

    private static ResponseStatus MapResponseStatusToDto(Application.Query.Queries.ResponseQueries.ResponseStatus status)
    {
        return status switch
        {
            Application.Query.Queries.ResponseQueries.ResponseStatus.NotStarted => ResponseStatus.NotStarted,
            Application.Query.Queries.ResponseQueries.ResponseStatus.InProgress => ResponseStatus.InProgress,
            Application.Query.Queries.ResponseQueries.ResponseStatus.Completed => ResponseStatus.Completed,
            Application.Query.Queries.ResponseQueries.ResponseStatus.Submitted => ResponseStatus.Submitted,
            _ => ResponseStatus.NotStarted
        };
    }

    private static IReadOnlyDictionary<Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus, QueryApi.Dto.AssignmentStatus> MapAssignmentStatusToDto =>
        new Dictionary<Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus, QueryApi.Dto.AssignmentStatus>
        {
            { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Assigned, QueryApi.Dto.AssignmentStatus.Assigned },
            { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Overdue, QueryApi.Dto.AssignmentStatus.Overdue },
            { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Cancelled, QueryApi.Dto.AssignmentStatus.Cancelled },
            { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.InProgress, QueryApi.Dto.AssignmentStatus.InProgress },
            { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Completed, QueryApi.Dto.AssignmentStatus.Completed },
        };
}