using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.CommandApi.Models;
using ti8m.BeachBreak.CommandApi.Services;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController : ControllerBase
{
    private readonly IQuestionnaireService _questionnaireService;
    private readonly ILogger<AssignmentsController> _logger;

    public AssignmentsController(
        IQuestionnaireService questionnaireService,
        ILogger<AssignmentsController> logger)
    {
        _questionnaireService = questionnaireService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestionnaireAssignment>>> GetAllAssignments()
    {
        try
        {
            var assignments = await _questionnaireService.GetAllAssignmentsAsync();
            return Ok(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments");
            return StatusCode(500, "An error occurred while retrieving assignments");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestionnaireAssignment>> GetAssignment(Guid id)
    {
        try
        {
            var assignment = await _questionnaireService.GetAssignmentByIdAsync(id);
            if (assignment == null)
                return NotFound($"Assignment with ID {id} not found");

            return Ok(assignment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignment {AssignmentId}", id);
            return StatusCode(500, "An error occurred while retrieving the assignment");
        }
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<List<QuestionnaireAssignment>>> GetAssignmentsByEmployee(string employeeId)
    {
        try
        {
            var assignments = await _questionnaireService.GetAssignmentsByEmployeeAsync(employeeId);
            return Ok(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving assignments");
        }
    }

    [HttpPost]
    public async Task<ActionResult<List<QuestionnaireAssignment>>> CreateAssignments(CreateAssignmentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.EmployeeIds == null || !request.EmployeeIds.Any())
                return BadRequest("At least one employee ID is required");

            var assignments = await _questionnaireService.CreateAssignmentsAsync(request);
            if (!assignments.Any())
                return BadRequest("Template not found or inactive");

            return CreatedAtAction(nameof(GetAllAssignments), assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assignments");
            return StatusCode(500, "An error occurred while creating assignments");
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<QuestionnaireAssignment>> UpdateAssignmentStatus(Guid id, [FromBody] AssignmentStatus status)
    {
        try
        {
            var assignment = await _questionnaireService.UpdateAssignmentStatusAsync(id, status);
            if (assignment == null)
                return NotFound($"Assignment with ID {id} not found");

            return Ok(assignment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating assignment status {AssignmentId}", id);
            return StatusCode(500, "An error occurred while updating assignment status");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAssignment(Guid id)
    {
        try
        {
            var success = await _questionnaireService.DeleteAssignmentAsync(id);
            if (!success)
                return NotFound($"Assignment with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assignment {AssignmentId}", id);
            return StatusCode(500, "An error occurred while deleting the assignment");
        }
    }
}