using Marten;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.ProgressQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.QueryApi.Dto;
// ARCHITECTURAL NOTE: QueryApi references Domain for shared enum types only (WorkflowState, CompletionRole, ResponseRole).
// This is pragmatic because Application.Query DTOs already use these Domain enums, and duplicating would cause ambiguity.
// FUTURE: Consider moving shared enums to Core layer for proper layering.
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/responses")]
public class ResponsesController : BaseController
{
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly IProgressCalculationService _progressCalculationService;
    private readonly IDocumentStore _documentStore;
    private readonly ILogger<ResponsesController> _logger;
    private readonly UserContext _userContext;

    public ResponsesController(
        IQueryDispatcher queryDispatcher,
        IProgressCalculationService progressCalculationService,
        IDocumentStore documentStore,
        ILogger<ResponsesController> logger,
        UserContext userContext)
    {
        _queryDispatcher = queryDispatcher;
        _progressCalculationService = progressCalculationService;
        _documentStore = documentStore;
        _logger = logger;
        _userContext = userContext;
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

            // Get current user's role for filtering
            if (!Guid.TryParse(_userContext.Id, out var userId))
            {
                _logger.LogWarning("GetResponseByAssignment: Unable to parse user ID from context");
                return CreateResponse(Result<QuestionnaireResponseDto>.Fail("User identification failed", 401));
            }

            var userRoleResult = await _queryDispatcher.QueryAsync(
                new GetEmployeeRoleByIdQuery(userId),
                HttpContext.RequestAborted);

            if (userRoleResult == null)
            {
                _logger.LogWarning("GetResponseByAssignment: User role not found for user {UserId}", userId);
                return CreateResponse(Result<QuestionnaireResponseDto>.Fail("User role not found", 403));
            }

            // Get assignment to check workflow state
            var assignmentQuery = new QuestionnaireAssignmentQuery(assignmentId);
            var assignmentResult = await _queryDispatcher.QueryAsync(assignmentQuery);

            if (assignmentResult?.Succeeded != true || assignmentResult.Payload == null)
            {
                return CreateResponse(Result<QuestionnaireResponseDto>.Fail("Assignment not found", 404));
            }

            // Get template to check section CompletionRoles
            var templateQuery = new QuestionnaireTemplateQuery(assignmentResult.Payload.TemplateId);
            var templateResult = await _queryDispatcher.QueryAsync(templateQuery);

            if (templateResult?.Succeeded != true || templateResult.Payload == null)
            {
                _logger.LogWarning("GetResponseByAssignment: Template not found for assignment {AssignmentId}", assignmentId);
                return CreateResponse(Result<QuestionnaireResponseDto>.Fail("Template not found", 404));
            }

            // Apply section and response filtering based on user role and workflow state
            var dto = MapToDto(response);
            dto = FilterSectionsByUserRoleAndWorkflowState(
                dto,
                assignmentResult.Payload,
                templateResult.Payload,
                userRoleResult.ApplicationRole);

            return CreateResponse(Result<QuestionnaireResponseDto>.Success(dto));
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

            // Calculate progress percentage using ReadModel (has full typed structure)
            var progressPercentage = 0;
            try
            {
                // Load ReadModel to get typed SectionResponses for progress calculation
                using var session = _documentStore.LightweightSession();
                var readModel = await session.Query<QuestionnaireResponseReadModel>()
                    .Where(r => r.AssignmentId == assignmentId)
                    .FirstOrDefaultAsync();

                if (readModel != null)
                {
                    // Get template for progress calculation
                    var templateQuery = new QuestionnaireTemplateQuery(response.TemplateId);
                    var templateResult = await _queryDispatcher.QueryAsync(templateQuery);
                    var template = templateResult?.Payload;

                    if (template != null)
                    {
                        var progress = _progressCalculationService.Calculate(template, readModel.SectionResponses);
                        progressPercentage = (int)Math.Round(progress.EmployeeProgress);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to calculate progress for assignment {AssignmentId}, defaulting to 0", assignmentId);
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
                ProgressPercentage = progressPercentage
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
            Dictionary<CompletionRole, Dictionary<Guid, object>>? roleBasedResponses = null;

            // Handle the fact that sectionKvp.Value might be a JsonElement or already the correct type
            if (sectionKvp.Value is System.Text.Json.JsonElement roleJsonElement)
            {
                roleBasedResponses = System.Text.Json.JsonSerializer.Deserialize<Dictionary<CompletionRole, Dictionary<Guid, object>>>(roleJsonElement.GetRawText());
            }
            else if (sectionKvp.Value is Dictionary<CompletionRole, Dictionary<Guid, object>> typedRoleResponses)
            {
                roleBasedResponses = typedRoleResponses;
            }

            if (roleBasedResponses == null) continue;

            // NEW: Populate RoleResponses with BOTH Employee and Manager responses (for review mode)
            var roleResponsesDto = new Dictionary<ResponseRole, Dictionary<Guid, QuestionResponseDto>>();

            foreach (var roleKvp in roleBasedResponses)
            {
                var role = roleKvp.Key;
                // Convert CompletionRole to ResponseRole
                ResponseRole responseRole = role switch
                {
                    CompletionRole.Employee => ResponseRole.Employee,
                    CompletionRole.Manager => ResponseRole.Manager,
                    _ => ResponseRole.Employee // Default fallback
                };
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
                    roleResponsesDto[responseRole] = questionResponsesForRole;
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
            Dictionary<CompletionRole, Dictionary<Guid, object>>? roleBasedResponses = null;

            // Handle the fact that sectionKvp.Value might be a JsonElement or already the correct type
            if (sectionKvp.Value is System.Text.Json.JsonElement roleJsonElement)
            {
                roleBasedResponses = System.Text.Json.JsonSerializer.Deserialize<Dictionary<CompletionRole, Dictionary<Guid, object>>>(roleJsonElement.GetRawText());
            }
            else if (sectionKvp.Value is Dictionary<CompletionRole, Dictionary<Guid, object>> typedRoleResponses)
            {
                roleBasedResponses = typedRoleResponses;
            }

            if (roleBasedResponses == null) continue;

            // For employee endpoints, return EMPLOYEE responses only
            var roleResponsesDto = new Dictionary<ResponseRole, Dictionary<Guid, QuestionResponseDto>>();

            if (roleBasedResponses.TryGetValue(CompletionRole.Employee, out var employeeResponses))
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
                    roleResponsesDto[ResponseRole.Employee] = questionResponsesForEmployee;
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

    /// <summary>
    /// Filters section responses based on user role and workflow state to prevent exposing data before it's ready.
    /// BUSINESS RULES:
    /// - Employees: See Employee + Both sections (but in Both sections, only their own Employee responses)
    /// - Managers: See Manager + Both sections (but in Both sections, only their own Manager responses)
    /// - InReview state: Manager sees ALL sections with ALL responses, Employee sees Employee + Both sections
    /// - ManagerReviewConfirmed onwards: Everyone sees ALL sections with ALL responses
    /// </summary>
    private QuestionnaireResponseDto FilterSectionsByUserRoleAndWorkflowState(
        QuestionnaireResponseDto response,
        Application.Query.Queries.QuestionnaireAssignmentQueries.QuestionnaireAssignment assignment,
        Application.Query.Queries.QuestionnaireTemplateQueries.QuestionnaireTemplate template,
        ApplicationRole userRole)
    {
        var isManager = userRole is ApplicationRole.TeamLead or ApplicationRole.HR or ApplicationRole.HRLead or ApplicationRole.Admin;

        // From ManagerReviewConfirmed onwards: Everyone sees ALL sections with ALL responses
        if (assignment.WorkflowState is WorkflowState.ManagerReviewConfirmed or WorkflowState.EmployeeReviewConfirmed or WorkflowState.Finalized)
        {
            return response; // Full transparency
        }

        // InReview state: Manager sees ALL, Employee sees only their sections
        if (assignment.WorkflowState == WorkflowState.InReview)
        {
            if (isManager)
            {
                return response; // Manager sees everything during review
            }
            // Employee continues with normal filtering (falls through)
        }

        // In-Progress + Submitted states: Filter by CompletionRole and ResponseRole
        var filteredSections = new Dictionary<Guid, SectionResponseDto>();

        foreach (var sectionKvp in response.SectionResponses)
        {
            var sectionId = sectionKvp.Key;
            var sectionDto = sectionKvp.Value;

            // Find section in template to get CompletionRole
            var templateSection = template.Sections.FirstOrDefault(s => s.Id == sectionId);
            if (templateSection == null) continue; // Skip sections not in template

            // Parse CompletionRole string to enum (template stores as string)
            if (!Enum.TryParse<CompletionRole>(templateSection.CompletionRole, out var completionRole))
            {
                continue; // Skip sections with invalid CompletionRole
            }

            // Determine if user should see this section
            bool shouldIncludeSection = false;
            Dictionary<ResponseRole, Dictionary<Guid, QuestionResponseDto>>? filteredRoleResponses = null;

            if (completionRole == CompletionRole.Both)
            {
                // Both sections: Everyone sees them, but filtered by their own responses
                shouldIncludeSection = true;
                filteredRoleResponses = new Dictionary<ResponseRole, Dictionary<Guid, QuestionResponseDto>>();

                // Filter to show only the user's own responses in Both sections
                if (isManager && sectionDto.RoleResponses.ContainsKey(ResponseRole.Manager))
                {
                    filteredRoleResponses[ResponseRole.Manager] = sectionDto.RoleResponses[ResponseRole.Manager];
                }
                else if (!isManager && sectionDto.RoleResponses.ContainsKey(ResponseRole.Employee))
                {
                    filteredRoleResponses[ResponseRole.Employee] = sectionDto.RoleResponses[ResponseRole.Employee];
                }
            }
            else if (isManager && completionRole == CompletionRole.Manager)
            {
                // Manager-only sections: Managers see all responses
                shouldIncludeSection = true;
                filteredRoleResponses = sectionDto.RoleResponses;
            }
            else if (!isManager && completionRole == CompletionRole.Employee)
            {
                // Employee-only sections: Employees see all responses
                shouldIncludeSection = true;
                filteredRoleResponses = sectionDto.RoleResponses;
            }

            if (shouldIncludeSection && filteredRoleResponses != null)
            {
                filteredSections[sectionId] = new SectionResponseDto
                {
                    RoleResponses = filteredRoleResponses
                };
            }
        }

        return new QuestionnaireResponseDto
        {
            Id = response.Id,
            AssignmentId = response.AssignmentId,
            TemplateId = response.TemplateId,
            EmployeeId = response.EmployeeId,
            SectionResponses = filteredSections,
            StartedDate = response.StartedDate,
            ProgressPercentage = response.ProgressPercentage
        };
    }

}