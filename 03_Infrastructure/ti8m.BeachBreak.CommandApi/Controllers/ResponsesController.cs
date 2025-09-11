using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.CommandApi.Models;
using ti8m.BeachBreak.CommandApi.Services;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/responses")]
public class ResponsesController : ControllerBase
{
    private readonly IQuestionnaireService _questionnaireService;
    private readonly ILogger<ResponsesController> logger;

    public ResponsesController(
        IQuestionnaireService questionnaireService,
        ILogger<ResponsesController> logger)
    {
        _questionnaireService = questionnaireService;
        this.logger = logger;
    }

    [HttpPost("assignment/{assignmentId:guid}")]
    public async Task<ActionResult<QuestionnaireResponse>> SaveResponse(
        Guid assignmentId, 
        [FromBody] Dictionary<Guid, SectionResponse> sectionResponses)
    {
        try
        {
            if (sectionResponses == null)
                return BadRequest("Section responses are required");

            var response = await _questionnaireService.CreateOrUpdateResponseAsync(assignmentId, sectionResponses);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid assignment ID {AssignmentId}", assignmentId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving response for assignment {AssignmentId}", assignmentId);
            return StatusCode(500, "An error occurred while saving the response");
        }
    }

    [HttpPost("assignment/{assignmentId:guid}/submit")]
    public async Task<ActionResult<QuestionnaireResponse>> SubmitResponse(Guid assignmentId)
    {
        try
        {
            var response = await _questionnaireService.SubmitResponseAsync(assignmentId);
            if (response == null)
                return NotFound($"Response for assignment {assignmentId} not found");

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting response for assignment {AssignmentId}", assignmentId);
            return StatusCode(500, "An error occurred while submitting the response");
        }
    }
}