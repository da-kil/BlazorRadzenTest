using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController : ControllerBase
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<AssignmentsController> logger;

    public AssignmentsController(
        IQueryDispatcher queryDispatcher,
        ILogger<AssignmentsController> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestionnaireAssignmentDto>>> GetAllAssignments()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new QuestionnaireAssignmentListQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments");
            return StatusCode(500, "An error occurred while retrieving assignments");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestionnaireAssignmentDto>> GetAssignment(Guid id)
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new QuestionnaireAssignmentQuery(id));
            if (result == null)
                return NotFound($"Assignment with ID {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignment {AssignmentId}", id);
            return StatusCode(500, "An error occurred while retrieving the assignment");
        }
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<List<QuestionnaireAssignmentDto>>> GetAssignmentsByEmployee(Guid employeeId)
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving assignments");
        }
    }
}