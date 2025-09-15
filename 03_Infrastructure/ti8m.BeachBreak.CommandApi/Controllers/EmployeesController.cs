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
    public async Task<IActionResult> BulkInsertEmployees([FromBody] IEnumerable<EmployeeDto> employeeDtos)
    {
        var employeeCount = employeeDtos?.Count() ?? 0;
        logger.LogInformation("Received bulk insert request for {EmployeeCount} employees", employeeCount);

        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Bulk insert request failed validation: {ValidationErrors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            if (!employeeDtos.Any())
            {
                logger.LogWarning("Bulk insert request received with no employees");
                return BadRequest("No employees provided for bulk insert");
            }

            var employees = employeeDtos.Select(dto => new Employee
            {
                Id = dto.Id,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Role = dto.Role.Trim(),
                EMail = dto.EMail.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                LastStartDate = dto.LastStartDate,
                ManagerId = dto.ManagerId,
                Manager = dto.Manager.Trim(),
                LoginName = dto.LoginName.Trim(),
                EmployeeNumber = dto.EmployeeNumber.Trim(),
                OrganizationNumber = dto.OrganizationNumber,
                Organization = dto.Organization.Trim(),
                IsDeleted = dto.IsDeleted
            });

            Result result = await commandDispatcher.SendAsync(new BulkInsertEmployeesCommand(employees));

            if (result.Succeeded)
            {
                logger.LogInformation("Bulk insert completed successfully for {EmployeeCount} employees", employeeCount);
            }
            else
            {
                logger.LogWarning("Bulk insert failed for {EmployeeCount} employees: {ErrorMessage}", employeeCount, result.Message);
            }

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk inserting employees");
            return StatusCode(500, "An error occurred while bulk inserting employees");
        }
    }

    [HttpPut("bulk-update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkUpdateEmployees([FromBody] IEnumerable<EmployeeDto> employeeDtos)
    {
        var employeeCount = employeeDtos?.Count() ?? 0;
        logger.LogInformation("Received bulk update request for {EmployeeCount} employees", employeeCount);

        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("Bulk update request failed validation: {ValidationErrors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            if (!employeeDtos.Any())
            {
                logger.LogWarning("Bulk update request received with no employees");
                return BadRequest("No employees provided for bulk update");
            }

            var employees = employeeDtos.Select(dto => new Employee
            {
                Id = dto.Id,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Role = dto.Role.Trim(),
                EMail = dto.EMail.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                LastStartDate = dto.LastStartDate,
                ManagerId = dto.ManagerId,
                Manager = dto.Manager.Trim(),
                LoginName = dto.LoginName.Trim(),
                EmployeeNumber = dto.EmployeeNumber.Trim(),
                OrganizationNumber = dto.OrganizationNumber,
                Organization = dto.Organization.Trim(),
                IsDeleted = dto.IsDeleted
            });

            Result result = await commandDispatcher.SendAsync(new BulkUpdateEmployeesCommand(employees));

            if (result.Succeeded)
            {
                logger.LogInformation("Bulk update completed successfully for {EmployeeCount} employees", employeeCount);
            }
            else
            {
                logger.LogWarning("Bulk update failed for {EmployeeCount} employees: {ErrorMessage}", employeeCount, result.Message);
            }

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk updating employees");
            return StatusCode(500, "An error occurred while bulk updating employees");
        }
    }

    [HttpDelete("bulk-delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkDeleteEmployees([FromBody] IEnumerable<Guid> employeeIds)
    {
        var employeeIdCount = employeeIds?.Count() ?? 0;
        logger.LogInformation("Received bulk delete request for {EmployeeIdCount} employee IDs", employeeIdCount);

        try
        {
            if (!employeeIds.Any())
            {
                logger.LogWarning("Bulk delete request received with no employee IDs");
                return BadRequest("No employee IDs provided for bulk delete");
            }

            Result result = await commandDispatcher.SendAsync(new BulkDeleteEmployeesCommand(employeeIds));

            if (result.Succeeded)
            {
                logger.LogInformation("Bulk delete completed successfully for {EmployeeIdCount} employee IDs", employeeIdCount);
            }
            else
            {
                logger.LogWarning("Bulk delete failed for {EmployeeIdCount} employee IDs: {ErrorMessage}", employeeIdCount, result.Message);
            }

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk deleting employees");
            return StatusCode(500, "An error occurred while bulk deleting employees");
        }
    }
}