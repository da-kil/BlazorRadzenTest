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

            if (employeeDtos == null || !employeeDtos.Any())
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

            if (employeeDtos == null || !employeeDtos.Any())
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
            if (employeeIds == null || !employeeIds.Any())
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

    // Employee-specific response management endpoints
    [HttpPost("{employeeId:guid}/responses/assignment/{assignmentId:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveEmployeeResponse(
        Guid employeeId,
        Guid assignmentId,
        [FromBody] Dictionary<Guid, object> sectionResponses)
    {
        logger.LogInformation("Received SaveEmployeeResponse request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);

        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("SaveEmployeeResponse request failed validation");
                return BadRequest(ModelState);
            }

            if (sectionResponses == null || !sectionResponses.Any())
            {
                logger.LogWarning("SaveEmployeeResponse request received with no section responses");
                return BadRequest("No section responses provided");
            }

            var command = new SaveEmployeeResponseCommand(employeeId, assignmentId, sectionResponses);
            var result = await commandDispatcher.SendAsync(command);

            if (result.Succeeded)
            {
                logger.LogInformation("SaveEmployeeResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);
                return Ok(result.Payload);
            }
            else
            {
                logger.LogWarning("SaveEmployeeResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}", employeeId, assignmentId, result.Message);
                return BadRequest(result.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving employee response for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);
            return StatusCode(500, "An error occurred while saving the employee response");
        }
    }

    [HttpPost("{employeeId:guid}/responses/assignment/{assignmentId:guid}/submit")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitEmployeeResponse(Guid employeeId, Guid assignmentId)
    {
        logger.LogInformation("Received SubmitEmployeeResponse request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);

        try
        {
            var command = new SubmitEmployeeResponseCommand(employeeId, assignmentId);
            var result = await commandDispatcher.SendAsync(command);

            if (result.Succeeded)
            {
                logger.LogInformation("SubmitEmployeeResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);
                return Ok(result.Message ?? "Response submitted successfully");
            }
            else
            {
                logger.LogWarning("SubmitEmployeeResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}", employeeId, assignmentId, result.Message);
                return BadRequest(result.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting employee response for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);
            return StatusCode(500, "An error occurred while submitting the employee response");
        }
    }
}