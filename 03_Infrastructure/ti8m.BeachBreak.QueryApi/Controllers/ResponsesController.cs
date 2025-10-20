using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.ProgressQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;
using ti8m.BeachBreak.QueryApi.Dto;

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
    [ProducesResponseType(typeof(List<QuestionnaireResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllResponses()
    {
        try
        {
            var query = new GetAllResponsesQuery();
            var responses = await _queryDispatcher.QueryAsync(query);
            var responseDtos = responses.Select(MapToDto).ToList();
            return CreateResponse(Result<List<QuestionnaireResponseDto>>.Success(responseDtos));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving responses");
            return CreateResponse(Result<List<QuestionnaireResponseDto>>.Fail("An error occurred while retrieving responses", 500));
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuestionnaireResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResponse(Guid id)
    {
        try
        {
            var query = new GetResponseByIdQuery(id);
            var response = await _queryDispatcher.QueryAsync(query);

            if (response == null)
                return CreateResponse(Result<QuestionnaireResponseDto>.Fail($"Response with ID {id} not found", 404));

            return CreateResponse(Result<QuestionnaireResponseDto>.Success(MapToDto(response)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving response {ResponseId}", id);
            return CreateResponse(Result<QuestionnaireResponseDto>.Fail("An error occurred while retrieving the response", 500));
        }
    }

    [HttpGet("assignment/{assignmentId:guid}")]
    [ProducesResponseType(typeof(QuestionnaireResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResponseByAssignment(Guid assignmentId)
    {
        try
        {
            var query = new GetResponseByAssignmentIdQuery(assignmentId);
            var response = await _queryDispatcher.QueryAsync(query);

            if (response == null)
                return CreateResponse(Result<QuestionnaireResponseDto>.Fail($"Response for assignment {assignmentId} not found", 404));

            return CreateResponse(Result<QuestionnaireResponseDto>.Success(MapToDto(response)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving response for assignment {AssignmentId}", assignmentId);
            return CreateResponse(Result<QuestionnaireResponseDto>.Fail("An error occurred while retrieving the response", 500));
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
            var result = await _queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId));

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
                    WorkflowState = assignment.WorkflowState,
                    AssignedDate = assignment.AssignedDate,
                    DueDate = assignment.DueDate,
                    CompletedDate = assignment.CompletedDate,
                    AssignedBy = assignment.AssignedBy,
                    Notes = assignment.Notes
                });
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", employeeId);
            return CreateResponse(Result<IEnumerable<QuestionnaireAssignmentDto>>.Fail("An error occurred while retrieving employee assignments", 500));
        }
    }

    [HttpGet("employee/{employeeId:guid}/assignment/{assignmentId:guid}")]
    [ProducesResponseType(typeof(QuestionnaireResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmployeeResponse(Guid employeeId, Guid assignmentId)
    {
        _logger.LogInformation("Received GetEmployeeResponse request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);

        try
        {
            // Use standard query handler with Marten read models
            var query = new GetResponseByAssignmentIdQuery(assignmentId);
            var response = await _queryDispatcher.QueryAsync(query);

            if (response == null)
            {
                _logger.LogInformation("Response not found for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);
                return CreateResponse(Result<QuestionnaireResponseDto>.Fail($"Response not found for assignment {assignmentId} and employee {employeeId}", 404));
            }

            // Validate this response belongs to the requesting employee (authorization check)
            if (response.EmployeeId != employeeId)
            {
                _logger.LogWarning("Employee {EmployeeId} attempted to access response for Assignment {AssignmentId} belonging to {ActualEmployeeId}",
                    employeeId, assignmentId, response.EmployeeId);
                return CreateResponse(Result<QuestionnaireResponseDto>.Fail("You do not have permission to access this response", 403));
            }

            // Map to DTO with employee-specific section responses
            var dto = new QuestionnaireResponseDto
            {
                Id = response.Id,
                TemplateId = response.TemplateId,
                AssignmentId = response.AssignmentId,
                EmployeeId = response.EmployeeId.ToString(),
                StartedDate = response.StartedDate,
                SectionResponses = MapEmployeeSectionResponsesToDto(response.SectionResponses),
                ProgressPercentage = 0 // TODO: Calculate progress percentage
            };

            return CreateResponse(Result<QuestionnaireResponseDto>.Success(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving response for assignment {AssignmentId} and employee {EmployeeId}", assignmentId, employeeId);
            return CreateResponse(Result<QuestionnaireResponseDto>.Fail("An error occurred while retrieving the employee response", 500));
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
            return CreateResponse(Result<IEnumerable<AssignmentProgressDto>>.Fail("An error occurred while retrieving employee assignment progress", 500));
        }
    }

    private QuestionnaireResponseDto MapToDto(QuestionnaireResponse response)
    {
        return new QuestionnaireResponseDto
        {
            Id = response.Id,
            AssignmentId = response.AssignmentId,
            TemplateId = response.TemplateId,
            EmployeeId = response.EmployeeId.ToString(),
            SectionResponses = MapSectionResponsesToDto(response.SectionResponses),
            StartedDate = response.StartedDate
        };
    }

    private Dictionary<Guid, SectionResponseDto> MapSectionResponsesToDto(Dictionary<Guid, object> sectionResponses)
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

            // NEW: Populate RoleResponses with BOTH Employee and Manager responses (for review mode)
            var roleResponsesDto = new Dictionary<string, Dictionary<Guid, QuestionResponseDto>>();

            foreach (var roleKvp in roleBasedResponses)
            {
                var role = roleKvp.Key;
                var roleKey = role.ToString(); // "Employee" or "Manager"
                var roleQuestions = roleKvp.Value;

                var questionResponsesForRole = new Dictionary<Guid, QuestionResponseDto>();

                foreach (var questionKvp in roleQuestions)
                {
                    var questionId = questionKvp.Key;
                    var responseValue = questionKvp.Value;

                    if (responseValue is System.Text.Json.JsonElement qJsonElement)
                    {
                        // Try to deserialize as QuestionResponseDto first
                        try
                        {
                            var questionResponse = System.Text.Json.JsonSerializer.Deserialize<QuestionResponseDto>(qJsonElement.GetRawText());
                            if (questionResponse != null && questionResponse.QuestionId != Guid.Empty)
                            {
                                // Successfully deserialized as structured response
                                questionResponsesForRole[questionId] = questionResponse;
                            }
                            else
                            {
                                // Failed to deserialize as QuestionResponseDto - treat as raw dictionary
                                var rawDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(qJsonElement.GetRawText());
                                questionResponsesForRole[questionId] = new QuestionResponseDto
                                {
                                    QuestionId = questionId,
                                    ComplexValue = rawDict
                                };
                            }
                        }
                        catch (System.Text.Json.JsonException jsonEx)
                        {
                            // Fallback: wrap as ComplexValue on JSON deserialization failure
                            _logger.LogWarning(jsonEx,
                                "Failed to deserialize question response for question {QuestionId} in section {SectionId}. Using fallback.",
                                questionId, sectionKvp.Key);
                            var fallbackDict = new Dictionary<string, object> { { "value", qJsonElement } };
                            questionResponsesForRole[questionId] = new QuestionResponseDto
                            {
                                QuestionId = questionId,
                                ComplexValue = fallbackDict
                            };
                        }
                    }
                    else if (responseValue is Dictionary<string, object> dict)
                    {
                        // Raw dictionary
                        questionResponsesForRole[questionId] = new QuestionResponseDto
                        {
                            QuestionId = questionId,
                            ComplexValue = dict
                        };
                    }
                    else
                    {
                        // Fallback: wrap any other type in ComplexValue
                        var valueDict = new Dictionary<string, object> { { "value", responseValue } };
                        questionResponsesForRole[questionId] = new QuestionResponseDto
                        {
                            QuestionId = questionId,
                            ComplexValue = valueDict
                        };
                    }
                }

                if (questionResponsesForRole.Any())
                {
                    roleResponsesDto[roleKey] = questionResponsesForRole;
                }
            }

            // Include section if it has any role responses
            if (roleResponsesDto.Any())
            {
                result[sectionId] = new SectionResponseDto
                {
                    SectionId = sectionId,
                    RoleResponses = roleResponsesDto
                };
            }
        }

        return result;
    }

    private Dictionary<Guid, SectionResponseDto> MapEmployeeSectionResponsesToDto(Dictionary<Guid, object> sectionResponses)
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

            // For employee endpoints, return EMPLOYEE responses only
            var roleResponsesDto = new Dictionary<string, Dictionary<Guid, QuestionResponseDto>>();

            if (roleBasedResponses.TryGetValue(Domain.QuestionnaireTemplateAggregate.CompletionRole.Employee, out var employeeResponses))
            {
                var questionResponsesForEmployee = new Dictionary<Guid, QuestionResponseDto>();

                // employeeResponses is Dictionary<Guid, object> where each value might be JsonElement
                foreach (var questionKvp in employeeResponses)
                {
                    var questionId = questionKvp.Key;
                    var responseValue = questionKvp.Value;

                    if (responseValue is System.Text.Json.JsonElement qJsonElement)
                    {
                        // Try to deserialize as QuestionResponseDto first
                        try
                        {
                            var questionResponse = System.Text.Json.JsonSerializer.Deserialize<QuestionResponseDto>(qJsonElement.GetRawText());
                            if (questionResponse != null && questionResponse.QuestionId != Guid.Empty)
                            {
                                // Successfully deserialized as structured response
                                questionResponsesForEmployee[questionId] = questionResponse;
                            }
                            else
                            {
                                // Failed to deserialize as QuestionResponseDto - treat as raw dictionary
                                var rawDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(qJsonElement.GetRawText());
                                questionResponsesForEmployee[questionId] = new QuestionResponseDto
                                {
                                    QuestionId = questionId,
                                    ComplexValue = rawDict
                                };
                            }
                        }
                        catch (System.Text.Json.JsonException jsonEx)
                        {
                            // Fallback: wrap as ComplexValue on JSON deserialization failure
                            _logger.LogWarning(jsonEx,
                                "Failed to deserialize employee question response for question {QuestionId}. Using fallback.",
                                questionId);
                            var fallbackDict = new Dictionary<string, object> { { "value", qJsonElement } };
                            questionResponsesForEmployee[questionId] = new QuestionResponseDto
                            {
                                QuestionId = questionId,
                                ComplexValue = fallbackDict
                            };
                        }
                    }
                    else if (responseValue is Dictionary<string, object> dict)
                    {
                        // Raw dictionary
                        questionResponsesForEmployee[questionId] = new QuestionResponseDto
                        {
                            QuestionId = questionId,
                            ComplexValue = dict
                        };
                    }
                    else
                    {
                        // Fallback: wrap any other type in ComplexValue
                        var valueDict = new Dictionary<string, object> { { "value", responseValue } };
                        questionResponsesForEmployee[questionId] = new QuestionResponseDto
                        {
                            QuestionId = questionId,
                            ComplexValue = valueDict
                        };
                    }
                }

                if (questionResponsesForEmployee.Any())
                {
                    roleResponsesDto["Employee"] = questionResponsesForEmployee;
                }
            }

            // Only include sections that have employee responses
            if (roleResponsesDto.Any())
            {
                result[sectionId] = new SectionResponseDto
                {
                    SectionId = sectionId,
                    RoleResponses = roleResponsesDto
                };
            }
        }

        return result;
    }

}