using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
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

    public EmployeesController(
        IQueryDispatcher queryDispatcher,
        ILogger<EmployeesController> logger,
        UserContext userContext)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
        this.userContext = userContext;
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
            var query = new EmployeeListQuery
            {
                IncludeDeleted = includeDeleted,
                OrganizationNumber = organizationNumber,
                Role = role,
                ManagerId = managerId
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
    /// Gets all questionnaire assignments for a specific employee by ID.
    /// This endpoint requires HR role as it allows viewing other employees' assignments.
    /// </summary>
    [HttpGet("{employeeId:guid}/assignments")]
    [Authorize(Roles = "TeamLead")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmployeeAssignments(Guid employeeId)
    {
        logger.LogInformation("Received GetEmployeeAssignments request for EmployeeId: {EmployeeId}", employeeId);

        try
        {
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