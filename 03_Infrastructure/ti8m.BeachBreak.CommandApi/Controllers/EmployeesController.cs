using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;
using ti8m.BeachBreak.CommandApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/employees")]
public class EmployeesController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly ILogger<EmployeesController> logger;

    public EmployeesController(
        ICommandDispatcher commandDispatcher,
        ILogger<EmployeesController> logger)
    {
        this.commandDispatcher = commandDispatcher;
        this.logger = logger;
    }

    [HttpPost("bulk-insert")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkDeleteEmployees([FromBody] IEnumerable<Guid> employeeIds)
    {
        Result result = await commandDispatcher.SendAsync(new BulkDeleteEmployeesCommand(employeeIds));

        return CreateResponse(result);
    }

}