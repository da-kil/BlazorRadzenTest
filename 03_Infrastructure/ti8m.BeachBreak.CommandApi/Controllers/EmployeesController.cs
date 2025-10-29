using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.CommandApi.Dto;
using ti8m.BeachBreak.CommandApi.Models;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using CommandResult = ti8m.BeachBreak.Application.Command.Commands.Result;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/employees")]
public class EmployeesController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly IQueryDispatcher queryDispatcher;
    private readonly UserContext userContext;
    private readonly ILogger<EmployeesController> logger;

    public EmployeesController(
        ICommandDispatcher commandDispatcher,
        IQueryDispatcher queryDispatcher,
        UserContext userContext,
        ILogger<EmployeesController> logger)
    {
        this.commandDispatcher = commandDispatcher;
        this.queryDispatcher = queryDispatcher;
        this.userContext = userContext;
        this.logger = logger;
    }

    [HttpPost("bulk-insert")]
    [Authorize(Policy = "AdminOrApp")] // Allows Admin users OR service principals with DataSeeder app role
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkInsertEmployees([FromBody] IEnumerable<EmployeeDto> employees)
    {
        CommandResult result = await commandDispatcher.SendAsync(new BulkInsertEmployeesCommand(
            employees.Select(dto => new SyncEmployee
            {
                Id = dto.Id,
                EmployeeId = dto.EmployeeId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = dto.Role,
                EMail = dto.EMail,
                StartDate = dto.StartDate,
                LastStartDate = dto.LastStartDate,
                EndDate = dto.EndDate,
                ManagerId = dto.ManagerId,
                LoginName = dto.LoginName,
                OrganizationNumber = dto.OrganizationNumber

            })));

        return CreateResponse(result);
    }

    [HttpPut("bulk-update")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkUpdateEmployees([FromBody] IEnumerable<EmployeeDto> employees)
    {
        CommandResult result = await commandDispatcher.SendAsync(new BulkUpdateEmployeesCommand(
            employees.Select(dto => new SyncEmployee
            {
                Id = dto.Id,
                EmployeeId = dto.EmployeeId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = dto.Role,
                EMail = dto.EMail,
                StartDate = dto.StartDate,
                LastStartDate = dto.LastStartDate,
                EndDate = dto.EndDate,
                ManagerId = dto.ManagerId,
                LoginName = dto.LoginName,
                OrganizationNumber = dto.OrganizationNumber

            })));

        return CreateResponse(result);
    }

    [HttpDelete("bulk-delete")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkDeleteEmployees([FromBody] IEnumerable<Guid> employeeIds)
    {
        CommandResult result = await commandDispatcher.SendAsync(new BulkDeleteEmployeesCommand(employeeIds));

        return CreateResponse(result);
    }

    /// <summary>
    /// Changes the application role of an employee.
    /// Only Admin, HRLead, and HR can access this endpoint.
    /// Controller fetches requester's role from database using UserContext; business rules enforced in domain layer.
    /// </summary>
    [HttpPut("{employeeId:guid}/application-role")]
    [Authorize(Policy = "HR")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeApplicationRole(
        Guid employeeId,
        [FromBody] ChangeApplicationRoleDto dto)
    {
        // Infrastructure responsibility: Fetch requester's role from database using UserContext
        if (!Guid.TryParse(userContext.Id, out var userId))
        {
            logger.LogWarning("Cannot change application role: User ID not found in UserContext");
            return CreateResponse(CommandResult.Fail("User identification failed", 401));
        }

        var requesterRoleResult = await queryDispatcher.QueryAsync(
            new GetEmployeeRoleByIdQuery(userId),
            HttpContext.RequestAborted);

        if (requesterRoleResult == null)
        {
            logger.LogWarning("Cannot change application role: Requester role not found for user {UserId}", userId);
            return CreateResponse(CommandResult.Fail("Requester role not found", 403));
        }

        // Dispatch command with requester's role - business rules validated in domain
        CommandResult result = await commandDispatcher.SendAsync(
            new ChangeEmployeeApplicationRoleCommand(
                employeeId,
                (ApplicationRole)dto.NewRole,
                requesterRoleResult.ApplicationRole));

        return CreateResponse(result);
    }

    /// <summary>
    /// Saves the currently authenticated employee's response to their assigned questionnaire.
    /// Uses UserContext to get the employee ID - more secure than accepting it as a parameter.
    /// This is the employee self-service endpoint that follows the "me" pattern for implicit authorization.
    /// Stores responses with CompletionRole.Employee in the domain model.
    /// </summary>
    [HttpPost("me/responses/assignment/{assignmentId:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveMyResponse(
        Guid assignmentId,
        [FromBody] Dictionary<Guid, SectionResponse> sectionResponses)
    {
        // Get employee ID from authenticated user context
        if (!Guid.TryParse(userContext.Id, out var employeeId))
        {
            logger.LogWarning("SaveMyResponse failed: Unable to parse user ID from context");
            return CreateResponse(Application.Command.Commands.Result<Guid>.Fail("User ID not found in authentication context", StatusCodes.Status401Unauthorized));
        }

        logger.LogInformation("Received SaveMyResponse request for authenticated EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
            employeeId, assignmentId);

        if (sectionResponses == null)
        {
            logger.LogWarning("SaveMyResponse failed: Section responses are null");
            return CreateResponse(Application.Command.Commands.Result<Guid>.Fail("Section responses are required", StatusCodes.Status400BadRequest));
        }

        // Extract role-based responses from each SectionResponse and flatten to domain structure
        var responsesAsObjects = sectionResponses.ToDictionary(
            kvp => kvp.Key,
            kvp => (object)kvp.Value.RoleResponses.ToDictionary(
                roleKvp => roleKvp.Key.ToString(), // Convert ResponseRole enum to string for domain layer
                roleKvp => (object)roleKvp.Value.ToDictionary(
                    q => q.Key,  // Question ID
                    q => (object)q.Value // QuestionResponse
                )
            )
        );

        var command = new SaveEmployeeResponseCommand(
            employeeId: employeeId,
            assignmentId: assignmentId,
            sectionResponses: responsesAsObjects
        );

        var result = await commandDispatcher.SendAsync(command);

        if (result.Succeeded)
        {
            logger.LogInformation("SaveMyResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, ResponseId: {ResponseId}",
                employeeId, assignmentId, result.Payload);
        }
        else
        {
            logger.LogWarning("SaveMyResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}",
                employeeId, assignmentId, result.Message);
        }

        return CreateResponse(result);
    }

    /// <summary>
    /// Submits the currently authenticated employee's response for their assigned questionnaire.
    /// Uses UserContext to get the employee ID - more secure than accepting it as a parameter.
    /// This is the employee self-service endpoint that follows the "me" pattern for implicit authorization.
    /// </summary>
    [HttpPost("me/responses/assignment/{assignmentId:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitMyResponse(Guid assignmentId)
    {
        // Get employee ID from authenticated user context
        if (!Guid.TryParse(userContext.Id, out var employeeId))
        {
            logger.LogWarning("SubmitMyResponse failed: Unable to parse user ID from context");
            return CreateResponse(CommandResult.Fail("User ID not found in authentication context", StatusCodes.Status401Unauthorized));
        }

        logger.LogInformation("Received SubmitMyResponse request for authenticated EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
            employeeId, assignmentId);

        var command = new SubmitEmployeeResponseCommand(
            employeeId: employeeId,
            assignmentId: assignmentId
        );

        var result = await commandDispatcher.SendAsync(command);

        if (result.Succeeded)
        {
            logger.LogInformation("SubmitMyResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                employeeId, assignmentId);
        }
        else
        {
            logger.LogWarning("SubmitMyResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}",
                employeeId, assignmentId, result.Message);
        }

        return CreateResponse(result);
    }

}