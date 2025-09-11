using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.QueryApi.Controllers;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/responses")]
public class ResponsesController : BaseController
{
    //private readonly IQuestionnaireService _questionnaireService;
    //private readonly ILogger<ResponsesController> _logger;

    //public ResponsesController(
    //    IQuestionnaireService questionnaireService,
    //    ILogger<ResponsesController> logger)
    //{
    //    _questionnaireService = questionnaireService;
    //    _logger = logger;
    //}

    //[HttpGet]
    //public async Task<ActionResult<List<QuestionnaireResponseDto>>> GetAllResponses()
    //{
    //    try
    //    {
    //        var responses = await _questionnaireService.GetAllResponsesAsync();
    //        return Ok(responses);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving responses");
    //        return StatusCode(500, "An error occurred while retrieving responses");
    //    }
    //}

    //[HttpGet("{id:guid}")]
    //public async Task<ActionResult<QuestionnaireResponseDto>> GetResponse(Guid id)
    //{
    //    try
    //    {
    //        var response = await _questionnaireService.GetResponseByIdAsync(id);
    //        if (response == null)
    //            return NotFound($"Response with ID {id} not found");

    //        return Ok(response);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving response {ResponseId}", id);
    //        return StatusCode(500, "An error occurred while retrieving the response");
    //    }
    //}

    //[HttpGet("assignment/{assignmentId:guid}")]
    //public async Task<ActionResult<QuestionnaireResponseDto>> GetResponseByAssignment(Guid assignmentId)
    //{
    //    try
    //    {
    //        var response = await _questionnaireService.GetResponseByAssignmentIdAsync(assignmentId);
    //        if (response == null)
    //            return NotFound($"Response for assignment {assignmentId} not found");

    //        return Ok(response);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving response for assignment {AssignmentId}", assignmentId);
    //        return StatusCode(500, "An error occurred while retrieving the response");
    //    }
    //}
}