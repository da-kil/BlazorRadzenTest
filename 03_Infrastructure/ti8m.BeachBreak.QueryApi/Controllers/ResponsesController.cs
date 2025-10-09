using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;
using ti8m.BeachBreak.QueryApi.Dto;
using ResponseStatusDto = ti8m.BeachBreak.QueryApi.Dto.ResponseStatus;
using ResponseStatusQuery = ti8m.BeachBreak.Application.Query.Queries.ResponseQueries.ResponseStatus;

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

    // Employee-specific questionnaire response endpoints
    [HttpGet("employee/{employeeId:guid}/assignments")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeAssignments(Guid employeeId)
    {
        _logger.LogInformation("Received GetEmployeeAssignments request for EmployeeId: {EmployeeId}", employeeId);

        try
        {
            var result = await _queryDispatcher.QueryAsync(new EmployeeAssignmentListQuery(employeeId));

            if (result.Succeeded && result.Payload != null)
            {
                _logger.LogInformation("GetEmployeeAssignments completed successfully for EmployeeId: {EmployeeId}, returned {Count} assignments", employeeId, result.Payload.Count());
            }
            else if (!result.Succeeded)
            {
                _logger.LogWarning("GetEmployeeAssignments failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}", employeeId, result.Message);
            }

            return CreateResponse(result, assignments =>
            {
                return assignments.Select(assignment => new QuestionnaireAssignmentDto
                {
                    Id = assignment.Id,
                    TemplateId = assignment.TemplateId,
                    EmployeeId = assignment.EmployeeId.ToString(),
                    EmployeeName = assignment.EmployeeName,
                    EmployeeEmail = assignment.EmployeeEmail,
                    AssignedDate = assignment.AssignedDate,
                    DueDate = assignment.DueDate,
                    CompletedDate = assignment.CompletedDate,
                    Status = MapAssignmentStatus(assignment.Status),
                    AssignedBy = assignment.AssignedBy,
                    Notes = assignment.Notes
                });
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving employee assignments");
        }
    }

    [HttpGet("employee/{employeeId:guid}/assignment/{assignmentId:guid}")]
    [ProducesResponseType(typeof(QuestionnaireResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeResponse(Guid employeeId, Guid assignmentId)
    {
        _logger.LogInformation("Received GetEmployeeResponse request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);

        try
        {
            var result = await _queryDispatcher.QueryAsync(new EmployeeResponseQuery(employeeId, assignmentId));

            if (result?.Payload == null)
            {
                _logger.LogInformation("Response not found for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);
                return NotFound($"Response not found for assignment {assignmentId} and employee {employeeId}");
            }

            return CreateResponse(result, response => new QuestionnaireResponseDto
            {
                Id = response.Id,
                TemplateId = response.TemplateId,
                AssignmentId = response.AssignmentId,
                EmployeeId = response.EmployeeId.ToString(),
                StartedDate = response.StartedDate,
                CompletedDate = response.SubmittedDate,
                Status = MapResponseStatus(response.Status),
                SectionResponses = response.SectionResponses.ToDictionary(kvp => kvp.Key, kvp => new SectionResponseDto { SectionId = kvp.Key }),
                ProgressPercentage = 0 // TODO: Calculate progress percentage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving response for assignment {AssignmentId} and employee {EmployeeId}", assignmentId, employeeId);
            return StatusCode(500, "An error occurred while retrieving the employee response");
        }
    }

    [HttpGet("employee/{employeeId:guid}/progress")]
    [ProducesResponseType(typeof(IEnumerable<AssignmentProgressDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeAssignmentProgress(Guid employeeId)
    {
        _logger.LogInformation("Received GetEmployeeAssignmentProgress request for EmployeeId: {EmployeeId}", employeeId);

        try
        {
            var result = await _queryDispatcher.QueryAsync(new EmployeeProgressQuery(employeeId));

            return CreateResponse(result, progressList =>
            {
                return progressList.Select(progress => new AssignmentProgressDto
                {
                    AssignmentId = progress.AssignmentId,
                    ProgressPercentage = progress.ProgressPercentage,
                    TotalQuestions = progress.TotalQuestions,
                    AnsweredQuestions = progress.AnsweredQuestions,
                    LastModified = progress.LastModified,
                    IsCompleted = progress.IsCompleted,
                    TimeSpent = progress.TimeSpent
                });
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignment progress for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving employee assignment progress");
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
            SectionResponses = MapSectionResponsesToDto(response.SectionResponses),
            CompletedDate = response.SubmittedDate,
            StartedDate = response.StartedDate
        };
    }

    private static Dictionary<Guid, SectionResponseDto> MapSectionResponsesToDto(Dictionary<Guid, object> sectionResponses)
    {
        var result = new Dictionary<Guid, SectionResponseDto>();

        foreach (var sectionKvp in sectionResponses)
        {
            var sectionId = sectionKvp.Key;
            Dictionary<Domain.QuestionnaireTemplateAggregate.CompletionRole, Dictionary<Guid, object>>? roleBasedResponses = null;

            // Handle the fact that sectionKvp.Value might be a JsonElement or already the correct type
            if (sectionKvp.Value is System.Text.Json.JsonElement roleJsonElement)
            {
                roleBasedResponses = System.Text.Json.JsonSerializer.Deserialize<Dictionary<Domain.QuestionnaireTemplateAggregate.CompletionRole, Dictionary<Guid, object>>>(roleJsonElement.GetRawText());
            }
            else if (sectionKvp.Value is Dictionary<Domain.QuestionnaireTemplateAggregate.CompletionRole, Dictionary<Guid, object>> typedRoleResponses)
            {
                roleBasedResponses = typedRoleResponses;
            }

            if (roleBasedResponses == null) continue;

            // For general response endpoints, return MANAGER responses
            // (This endpoint is primarily used by managers viewing responses)
            var questionResponsesDict = new Dictionary<Guid, QuestionResponseDto>();

            if (roleBasedResponses.TryGetValue(Domain.QuestionnaireTemplateAggregate.CompletionRole.Manager, out var managerResponses))
            {
                // managerResponses is Dictionary<Guid, object> where each value might be JsonElement
                foreach (var questionKvp in managerResponses)
                {
                    var questionId = questionKvp.Key;
                    var responseValue = questionKvp.Value;

                    if (responseValue is System.Text.Json.JsonElement qJsonElement)
                    {
                        var questionResponse = System.Text.Json.JsonSerializer.Deserialize<QuestionResponseDto>(qJsonElement.GetRawText());
                        if (questionResponse != null)
                        {
                            questionResponsesDict[questionId] = questionResponse;
                        }
                    }
                    else
                    {
                        // Fallback: create a simple response
                        questionResponsesDict[questionId] = new QuestionResponseDto
                        {
                            QuestionId = questionId,
                            Value = responseValue
                        };
                    }
                }
            }

            // Only include sections that have manager responses
            if (questionResponsesDict.Any())
            {
                result[sectionId] = new SectionResponseDto
                {
                    SectionId = sectionId,
                    QuestionResponses = questionResponsesDict
                };
            }
        }

        return result;
    }

    // Helper methods for status mapping
    private static AssignmentStatus MapAssignmentStatus(Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus status)
    {
        return status switch
        {
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Assigned => AssignmentStatus.Assigned,
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.InProgress => AssignmentStatus.InProgress,
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Completed => AssignmentStatus.Completed,
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Overdue => AssignmentStatus.Overdue,
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Cancelled => AssignmentStatus.Cancelled,
            _ => AssignmentStatus.Assigned
        };
    }

    private static ResponseStatusDto MapResponseStatus(ResponseStatusQuery status)
    {
        return status switch
        {
            ResponseStatusQuery.NotStarted => ResponseStatusDto.NotStarted,
            ResponseStatusQuery.InProgress => ResponseStatusDto.InProgress,
            ResponseStatusQuery.Completed => ResponseStatusDto.Completed,
            ResponseStatusQuery.Submitted => ResponseStatusDto.Submitted,
            _ => ResponseStatusDto.NotStarted
        };
    }
}