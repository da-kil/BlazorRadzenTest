using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.CommandApi.Models;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/responses")]
public class ResponsesController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly UserContext userContext;
    private readonly ILogger<ResponsesController> logger;

    public ResponsesController(
        ICommandDispatcher commandDispatcher,
        UserContext userContext,
        ILogger<ResponsesController> logger)
    {
        this.commandDispatcher = commandDispatcher;
        this.userContext = userContext;
        this.logger = logger;
    }

    [HttpPost("assignment/{assignmentId:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> SaveResponse(
        Guid assignmentId,
        [FromBody] Dictionary<Guid, SectionResponse> sectionResponses)
    {
        // Get employee ID from authenticated user context
        if (!Guid.TryParse(userContext.Id, out var employeeId))
        {
            logger.LogWarning("SaveResponse failed: Unable to parse user ID from context");
            return Unauthorized("User ID not found in authentication context");
        }

        logger.LogInformation("Received SaveResponse request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
            employeeId, assignmentId);

        try
        {
            if (sectionResponses == null)
            {
                logger.LogWarning("SaveResponse failed: Section responses are null");
                return BadRequest("Section responses are required");
            }

            // Extract QuestionResponses from each SectionResponse
            var responsesAsObjects = sectionResponses.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)kvp.Value.QuestionResponses.ToDictionary(
                    q => q.Key,
                    q => (object)q.Value
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
                logger.LogInformation("SaveResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, ResponseId: {ResponseId}",
                    employeeId, assignmentId, result.Payload);
                return Ok(result.Payload);
            }

            logger.LogWarning("SaveResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}",
                employeeId, assignmentId, result.Message);
            return StatusCode(result.StatusCode, result.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving response for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                employeeId, assignmentId);
            return StatusCode(500, "An error occurred while saving the response");
        }
    }

    [HttpPost("assignment/{assignmentId:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitResponse(Guid assignmentId)
    {
        // Get employee ID from authenticated user context
        if (!Guid.TryParse(userContext.Id, out var employeeId))
        {
            logger.LogWarning("SubmitResponse failed: Unable to parse user ID from context");
            return Unauthorized("User ID not found in authentication context");
        }

        logger.LogInformation("Received SubmitResponse request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
            employeeId, assignmentId);

        try
        {
            var command = new SubmitEmployeeResponseCommand(
                employeeId: employeeId,
                assignmentId: assignmentId
            );

            var result = await commandDispatcher.SendAsync(command);

            if (result.Succeeded)
            {
                logger.LogInformation("SubmitResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                    employeeId, assignmentId);
                return Ok(new { message = result.Message });
            }

            logger.LogWarning("SubmitResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}",
                employeeId, assignmentId, result.Message);
            return StatusCode(result.StatusCode, result.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting response for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                employeeId, assignmentId);
            return StatusCode(500, "An error occurred while submitting the response");
        }
    }
}