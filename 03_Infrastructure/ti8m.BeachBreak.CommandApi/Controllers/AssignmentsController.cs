using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;
using ti8m.BeachBreak.CommandApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/assignments")]
public class AssignmentsController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly ILogger<AssignmentsController> logger;

    public AssignmentsController(
        ICommandDispatcher commandDispatcher,
        ILogger<AssignmentsController> logger)
    {
        this.commandDispatcher = commandDispatcher;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAssignments(QuestionnaireAssignmentDto questionnaireAssignment)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (questionnaireAssignment.EmployeeIds == null || !questionnaireAssignment.EmployeeIds.Any())
                return BadRequest("At least one employee ID is required");

            Result result = await commandDispatcher.SendAsync(new CreateQuestionnaireAssignmentCommand(
                new QuestionnaireAssignment
                {
                    AssignedBy = questionnaireAssignment.AssignedBy,
                    DueDate = questionnaireAssignment.DueDate,
                    EmployeeIds = questionnaireAssignment.EmployeeIds,
                    Notes = questionnaireAssignment.Notes,
                    TemplateId = questionnaireAssignment.TemplateId
                }));

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating assignments");
            return StatusCode(500, "An error occurred while creating assignments");
        }
    }

    //[HttpPatch("{id:guid}/status")]
    //public async Task<ActionResult<QuestionnaireAssignment>> UpdateAssignmentStatus(Guid id, [FromBody] AssignmentStatus status)
    //{
    //    try
    //    {
    //        var assignment = await _questionnaireService.UpdateAssignmentStatusAsync(id, status);
    //        if (assignment == null)
    //            return NotFound($"Assignment with ID {id} not found");

    //        return Ok(assignment);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error updating assignment status {AssignmentId}", id);
    //        return StatusCode(500, "An error occurred while updating assignment status");
    //    }
    //}

    //[HttpDelete("{id:guid}")]
    //public async Task<IActionResult> DeleteAssignment(Guid id)
    //{
    //    try
    //    {
    //        var success = await _questionnaireService.DeleteAssignmentAsync(id);
    //        if (!success)
    //            return NotFound($"Assignment with ID {id} not found");

    //        return NoContent();
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error deleting assignment {AssignmentId}", id);
    //        return StatusCode(500, "An error occurred while deleting the assignment");
    //    }
    //}
}