using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/employees")]
[Authorize] // All endpoints require authentication
public class EmployeesController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<EmployeesController> logger;

    public EmployeesController(
        IQueryDispatcher queryDispatcher,
        ILogger<EmployeesController> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
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

}