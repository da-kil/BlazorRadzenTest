using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.CommandApi.Dto;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/employees")]
//[Authorize] // All endpoints require authentication
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
    //[Authorize(Roles = "HRAccess")] // Only Admin, HRLead, HR can create employees
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkInsertEmployees([FromBody] IEnumerable<EmployeeDto> employees)
    {
        Result result = await commandDispatcher.SendAsync(new BulkInsertEmployeesCommand(
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
    [Authorize(Roles = "HRAccess")] // Only Admin, HRLead, HR can update employees
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkUpdateEmployees([FromBody] IEnumerable<EmployeeDto> employees)
    {
        Result result = await commandDispatcher.SendAsync(new BulkUpdateEmployeesCommand(
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
    [Authorize(Roles = "HRLeadOnly")] // Only Admin, HRLead can delete employees
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkDeleteEmployees([FromBody] IEnumerable<Guid> employeeIds)
    {
        Result result = await commandDispatcher.SendAsync(new BulkDeleteEmployeesCommand(employeeIds));

        return CreateResponse(result);
    }

    /// <summary>
    /// Changes the application role of an employee.
    /// Only Admin, HRLead, and HR can access this endpoint.
    /// Controller fetches requester's role from database using UserContext; business rules enforced in domain layer.
    /// </summary>
    [HttpPut("{employeeId:guid}/application-role")]
    [Authorize(Roles = "HR")]
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
            return CreateResponse(Result.Fail("User identification failed", 401));
        }

        var requesterRoleResult = await queryDispatcher.QueryAsync(
            new GetEmployeeRoleByIdQuery(userId),
            HttpContext.RequestAborted);

        if (requesterRoleResult == null)
        {
            logger.LogWarning("Cannot change application role: Requester role not found for user {UserId}", userId);
            return CreateResponse(Result.Fail("Requester role not found", 403));
        }

        // Dispatch command with requester's role - business rules validated in domain
        Result result = await commandDispatcher.SendAsync(
            new ChangeEmployeeApplicationRoleCommand(
                employeeId,
                (ApplicationRole)dto.NewRole,
                requesterRoleResult.ApplicationRole));

        return CreateResponse(result);
    }

}