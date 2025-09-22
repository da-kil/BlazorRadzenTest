using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.QueryApi.Controllers;
using ti8m.BeachBreak.QueryApi.Dto;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;
using ResponseStatusQuery = ti8m.BeachBreak.Application.Query.Queries.ResponseQueries.ResponseStatus;
using ResponseStatusDto = ti8m.BeachBreak.QueryApi.Dto.ResponseStatus;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/responses")]
public class ResponsesController : BaseController
{
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly ILogger<ResponsesController> _logger;

    public ResponsesController(
        IQueryDispatcher queryDispatcher,
        ILogger<ResponsesController> logger)
    {
        _queryDispatcher = queryDispatcher;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestionnaireResponseDto>>> GetAllResponses()
    {
        try
        {
            var query = new GetAllResponsesQuery();
            var responses = await _queryDispatcher.QueryAsync(query);
            var responseDtos = responses.Select(MapToDto).ToList();
            return Ok(responseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving responses");
            return StatusCode(500, "An error occurred while retrieving responses");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestionnaireResponseDto>> GetResponse(Guid id)
    {
        try
        {
            var query = new GetResponseByIdQuery(id);
            var response = await _queryDispatcher.QueryAsync(query);

            if (response == null)
                return NotFound($"Response with ID {id} not found");

            return Ok(MapToDto(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving response {ResponseId}", id);
            return StatusCode(500, "An error occurred while retrieving the response");
        }
    }

    [HttpGet("assignment/{assignmentId:guid}")]
    public async Task<ActionResult<QuestionnaireResponseDto>> GetResponseByAssignment(Guid assignmentId)
    {
        try
        {
            var query = new GetResponseByAssignmentIdQuery(assignmentId);
            var response = await _queryDispatcher.QueryAsync(query);

            if (response == null)
                return NotFound($"Response for assignment {assignmentId} not found");

            return Ok(MapToDto(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving response for assignment {AssignmentId}", assignmentId);
            return StatusCode(500, "An error occurred while retrieving the response");
        }
    }

    private static QuestionnaireResponseDto MapToDto(QuestionnaireResponse response)
    {
        return new QuestionnaireResponseDto
        {
            Id = response.Id,
            AssignmentId = response.AssignmentId,
            TemplateId = response.TemplateId,
            EmployeeId = response.EmployeeId.ToString(),
            Status = (ResponseStatusDto)response.Status,
            SectionResponses = response.SectionResponses.ToDictionary(
                kvp => kvp.Key,
                kvp => MapToSectionResponseDto(kvp.Value)
            ),
            CompletedDate = response.SubmittedDate,
            StartedDate = response.StartedDate
        };
    }

    private static SectionResponseDto MapToSectionResponseDto(object sectionResponse)
    {
        // For now, return a basic mapping - this could be enhanced based on actual structure
        return new SectionResponseDto
        {
            SectionId = Guid.Empty, // This would need to be extracted from the response
            QuestionResponses = new Dictionary<Guid, QuestionResponseDto>()
        };
    }
}