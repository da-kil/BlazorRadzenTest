using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.CommandApi.Models;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/responses")]
public class ResponsesController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly ILogger<ResponsesController> _logger;

    public ResponsesController(
        ICommandDispatcher commandDispatcher,
        ILogger<ResponsesController> logger)
    {
        _commandDispatcher = commandDispatcher;
        _logger = logger;
    }

    [HttpPost("assignment/{assignmentId:guid}")]
    public async Task<ActionResult<Guid>> SaveResponse(
        Guid assignmentId,
        [FromBody] Dictionary<Guid, SectionResponse> sectionResponses)
    {
        try
        {
            if (sectionResponses == null)
                return BadRequest("Section responses are required");

            // Convert SectionResponse to generic object dictionary
            var responsesAsObjects = sectionResponses.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)kvp.Value
            );

            var command = new SaveEmployeeResponseCommand(
                employeeId: new Guid("b0f388c2-6294-4116-a8b2-eccafa29b3fb"), // TODO: Get from user context
                assignmentId: assignmentId,
                sectionResponses: responsesAsObjects,
                status: ti8m.BeachBreak.Application.Command.Commands.ResponseStatus.InProgress
            );

            var result = await _commandDispatcher.SendAsync(command);

            if (result.Succeeded)
            {
                return Ok(result.Payload);
            }

            return StatusCode(result.StatusCode, result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving response for assignment {AssignmentId}", assignmentId);
            return StatusCode(500, "An error occurred while saving the response");
        }
    }

    [HttpPost("assignment/{assignmentId:guid}/submit")]
    public async Task<ActionResult> SubmitResponse(Guid assignmentId)
    {
        try
        {
            var command = new SubmitEmployeeResponseCommand(
                employeeId: new Guid("b0f388c2-6294-4116-a8b2-eccafa29b3fb"), // TODO: Get from user context
                assignmentId: assignmentId
            );

            var result = await _commandDispatcher.SendAsync(command);

            if (result.Succeeded)
            {
                return Ok(new { message = result.Message });
            }

            return StatusCode(result.StatusCode, result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting response for assignment {AssignmentId}", assignmentId);
            return StatusCode(500, "An error occurred while submitting the response");
        }
    }
}