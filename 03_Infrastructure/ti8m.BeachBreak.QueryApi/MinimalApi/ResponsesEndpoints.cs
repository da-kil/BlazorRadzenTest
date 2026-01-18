using Marten;
using System.Text.Json;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.ProgressQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
using ti8m.BeachBreak.Infrastructure.Marten.JsonSerialization;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for response queries.
/// </summary>
public static class ResponsesEndpoints
{
    /// <summary>
    /// Maps response query endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapResponsesEndpoints(this WebApplication app)
    {
        var responsesGroup = app.MapGroup("/q/api/v{version:apiVersion}/responses")
            .WithTags("Responses")
            .RequireAuthorization(); // All endpoints require authentication

        // Get all responses
        responsesGroup.MapGet("/", async (
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var query = new GetAllResponsesQuery();
                var responses = await queryDispatcher.QueryAsync(query, cancellationToken);
                var responseDtos = responses.Select(MapToDto).ToList();
                return Results.Ok(responseDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving responses");
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving responses",
                    statusCode: 500);
            }
        })
        .WithName("GetAllResponses")
        .WithSummary("Get all responses")
        .WithDescription("Gets all questionnaire responses")
        .Produces<List<QuestionnaireResponseDto>>(200)
        .Produces(500);

        // Get response by ID
        responsesGroup.MapGet("/{id:guid}", async (
            Guid id,
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var query = new GetResponseByIdQuery(id);
                var response = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (response == null)
                    return Results.NotFound($"Response with ID {id} not found");

                return Results.Ok(MapToDto(response));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving response {ResponseId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving the response",
                    statusCode: 500);
            }
        })
        .WithName("GetResponse")
        .WithSummary("Get response by ID")
        .WithDescription("Gets a specific questionnaire response by ID")
        .Produces<QuestionnaireResponseDto>(200)
        .Produces(404)
        .Produces(500);

        // Get response by assignment with role-based filtering
        responsesGroup.MapGet("/assignment/{assignmentId:guid}", async (
            Guid assignmentId,
            IQueryDispatcher queryDispatcher,
            UserContext userContext,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var query = new GetResponseByAssignmentIdQuery(assignmentId);
                var response = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (response == null)
                    return Results.NotFound($"Response for assignment {assignmentId} not found");

                // Get current user's role for filtering
                if (!Guid.TryParse(userContext.Id, out var userId))
                {
                    logger.LogWarning("GetResponseByAssignment: Unable to parse user ID from context");
                    return Results.Unauthorized();
                }

                var userRoleResult = await queryDispatcher.QueryAsync(
                    new GetEmployeeRoleByIdQuery(userId), cancellationToken);

                if (userRoleResult == null)
                {
                    logger.LogWarning("GetResponseByAssignment: User role not found for user {UserId}", userId);
                    return Results.Forbid();
                }

                // Get assignment to check workflow state
                var assignmentQuery = new QuestionnaireAssignmentQuery(assignmentId);
                var assignmentResult = await queryDispatcher.QueryAsync(assignmentQuery, cancellationToken);

                if (assignmentResult?.Succeeded != true || assignmentResult.Payload == null)
                {
                    return Results.NotFound("Assignment not found");
                }

                // Get template to check section CompletionRoles
                var templateQuery = new QuestionnaireTemplateQuery(assignmentResult.Payload.TemplateId);
                var templateResult = await queryDispatcher.QueryAsync(templateQuery, cancellationToken);

                if (templateResult?.Succeeded != true || templateResult.Payload == null)
                {
                    logger.LogWarning("GetResponseByAssignment: Template not found for assignment {AssignmentId}", assignmentId);
                    return Results.NotFound("Template not found");
                }

                // Apply section and response filtering based on user role and workflow state
                var dto = MapToDto(response);
                dto = FilterSectionsByUserRoleAndWorkflowState(
                    dto,
                    assignmentResult.Payload,
                    templateResult.Payload,
                    userRoleResult.ApplicationRole);

                return Results.Ok(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving response for assignment {AssignmentId}", assignmentId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving the response",
                    statusCode: 500);
            }
        })
        .WithName("GetResponseByAssignment")
        .WithSummary("Get response by assignment")
        .WithDescription("Gets a questionnaire response by assignment ID with role-based filtering")
        .Produces<QuestionnaireResponseDto>(200)
        .Produces(404)
        .Produces(500);

        // Get employee assignments
        responsesGroup.MapGet("/employee/{employeeId:guid}/assignments", async (
            Guid employeeId,
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            logger.LogInformation("Received GetEmployeeAssignments request for EmployeeId: {EmployeeId}", employeeId);

            try
            {
                var result = await queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId), cancellationToken);

                if (result.Succeeded && result.Payload != null)
                {
                    logger.LogInformation("GetEmployeeAssignments completed successfully for EmployeeId: {EmployeeId}, returned {Count} assignments",
                        employeeId, result.Payload.Count());

                    var assignments = result.Payload.Select(assignment => new QuestionnaireAssignmentDto
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
                        Notes = assignment.Notes,

                        // InReview notes system
                        InReviewNotes = assignment.InReviewNotes.Select(note => new ti8m.BeachBreak.QueryApi.Dto.InReviewNoteDto
                        {
                            Id = note.Id,
                            Content = note.Content,
                            Timestamp = note.Timestamp,
                            SectionId = note.SectionId,
                            SectionTitle = note.SectionTitle,
                            AuthorEmployeeId = note.AuthorEmployeeId,
                            AuthorName = note.AuthorName
                        }).ToList()
                    });

                    return Results.Ok(assignments);
                }
                else if (!result.Succeeded)
                {
                    logger.LogWarning("GetEmployeeAssignments failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}",
                        employeeId, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }

                return Results.Ok(Enumerable.Empty<QuestionnaireAssignmentDto>());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", employeeId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving employee assignments",
                    statusCode: 500);
            }
        })
        .WithName("GetEmployeeAssignments")
        .WithSummary("Get employee assignments")
        .WithDescription("Gets all questionnaire assignments for a specific employee")
        .Produces<IEnumerable<QuestionnaireAssignmentDto>>(200)
        .Produces(500);

        // Get employee response for specific assignment
        responsesGroup.MapGet("/employee/{employeeId:guid}/assignment/{assignmentId:guid}", async (
            Guid employeeId,
            Guid assignmentId,
            IQueryDispatcher queryDispatcher,
            IProgressCalculationService progressCalculationService,
            IDocumentStore documentStore,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            logger.LogInformation("Received GetEmployeeResponse request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                employeeId, assignmentId);

            try
            {
                // Use standard query handler with Marten read models
                var query = new GetResponseByAssignmentIdQuery(assignmentId);
                var response = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (response == null)
                {
                    logger.LogInformation("Response not found for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                        employeeId, assignmentId);
                    return Results.NotFound($"Response not found for assignment {assignmentId} and employee {employeeId}");
                }

                // Validate this response belongs to the requesting employee (authorization check)
                if (response.EmployeeId != employeeId)
                {
                    logger.LogWarning("Employee {EmployeeId} attempted to access response for Assignment {AssignmentId} belonging to {ActualEmployeeId}",
                        employeeId, assignmentId, response.EmployeeId);
                    return Results.Forbid();
                }

                // Calculate progress percentage using ReadModel (has full typed structure)
                var progressPercentage = 0;
                try
                {
                    // Load ReadModel to get typed SectionResponses for progress calculation
                    using var session = documentStore.LightweightSession();
                    var readModel = await session.Query<QuestionnaireResponseReadModel>()
                        .Where(r => r.AssignmentId == assignmentId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (readModel != null)
                    {
                        // Get template for progress calculation
                        var templateQuery = new QuestionnaireTemplateQuery(response.TemplateId);
                        var templateResult = await queryDispatcher.QueryAsync(templateQuery, cancellationToken);
                        var template = templateResult?.Payload;

                        if (template != null)
                        {
                            var progress = progressCalculationService.Calculate(template, readModel.SectionResponses);
                            progressPercentage = (int)Math.Round(progress.EmployeeProgress);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to calculate progress for assignment {AssignmentId}, defaulting to 0", assignmentId);
                }

                // Map to DTO with employee-specific section responses
                var dto = new QuestionnaireResponseDto
                {
                    Id = response.Id,
                    TemplateId = response.TemplateId,
                    AssignmentId = response.AssignmentId,
                    EmployeeId = response.EmployeeId.ToString(),
                    StartedDate = response.StartedDate,
                    SectionResponses = MapStronglyTypedEmployeeSectionResponsesToDto(response.SectionResponses),
                    ProgressPercentage = progressPercentage
                };

                return Results.Ok(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving response for assignment {AssignmentId} and employee {EmployeeId}",
                    assignmentId, employeeId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving the employee response",
                    statusCode: 500);
            }
        })
        .WithName("GetEmployeeResponse")
        .WithSummary("Get employee response")
        .WithDescription("Gets a questionnaire response for a specific employee and assignment")
        .Produces<QuestionnaireResponseDto>(200)
        .Produces(404)
        .Produces(403)
        .Produces(500);

        // Get employee assignment progress
        responsesGroup.MapGet("/employee/{employeeId:guid}/progress", async (
            Guid employeeId,
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            logger.LogInformation("Received GetEmployeeAssignmentProgress request for EmployeeId: {EmployeeId}", employeeId);

            try
            {
                var result = await queryDispatcher.QueryAsync(new EmployeeProgressQuery(employeeId), cancellationToken);

                if (result.Succeeded)
                {
                    var progressList = result.Payload.Select(progress => new AssignmentProgressDto
                    {
                        AssignmentId = progress.AssignmentId,
                        ProgressPercentage = progress.ProgressPercentage,
                        TotalQuestions = progress.TotalQuestions,
                        AnsweredQuestions = progress.AnsweredQuestions,
                        LastModified = progress.LastModified,
                        IsCompleted = progress.IsCompleted,
                        TimeSpent = progress.TimeSpent
                    });

                    return Results.Ok(progressList);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving assignment progress for employee {EmployeeId}", employeeId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving employee assignment progress",
                    statusCode: 500);
            }
        })
        .WithName("GetEmployeeAssignmentProgress")
        .WithSummary("Get employee assignment progress")
        .WithDescription("Gets assignment progress for a specific employee")
        .Produces<IEnumerable<AssignmentProgressDto>>(200)
        .Produces(500);
    }

    private static QuestionnaireResponseDto MapToDto(QuestionnaireResponse response)
    {
        return new QuestionnaireResponseDto
        {
            Id = response.Id,
            AssignmentId = response.AssignmentId,
            TemplateId = response.TemplateId,
            EmployeeId = response.EmployeeId.ToString(),
            SectionResponses = MapStronglyTypedSectionResponsesToDto(response.SectionResponses),
            StartedDate = response.StartedDate
        };
    }

    /// <summary>
    /// Maps strongly-typed section responses directly to DTO format.
    /// Much cleaner than the object-based approach since we have compile-time type safety.
    /// </summary>
    private static Dictionary<Guid, SectionResponseDto> MapStronglyTypedSectionResponsesToDto(
        Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>> sectionResponses)
    {
        var result = new Dictionary<Guid, SectionResponseDto>();
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            Converters = { new QuestionResponseValueJsonConverter() }
        };

        foreach (var sectionKvp in sectionResponses)
        {
            var sectionId = sectionKvp.Key;
            var roleBasedResponses = sectionKvp.Value;

            var roleResponsesDto = new Dictionary<ResponseRole, Dictionary<Guid, QuestionResponseDto>>();

            foreach (var roleKvp in roleBasedResponses)
            {
                var completionRole = roleKvp.Key;

                // Convert CompletionRole to ResponseRole
                ResponseRole responseRole = completionRole switch
                {
                    CompletionRole.Employee => ResponseRole.Employee,
                    CompletionRole.Manager => ResponseRole.Manager,
                    _ => ResponseRole.Employee // Default fallback
                };

                var roleResponse = roleKvp.Value;
                var questionResponsesForRole = new Dictionary<Guid, QuestionResponseDto>();

                // Direct assignment - Section IS the question (one response per section)
                questionResponsesForRole[sectionId] = new QuestionResponseDto
                {
                    QuestionId = sectionId,
                    QuestionType = QuestionResponseMapper.InferQuestionType(roleResponse),
                    ResponseData = QuestionResponseMapper.MapToDto(roleResponse)
                };

                roleResponsesDto[responseRole] = questionResponsesForRole;
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

    /// <summary>
    /// Maps strongly-typed section responses to DTO format, showing only Employee responses.
    /// Used for employee-specific endpoints that should only show their own responses.
    /// </summary>
    private static Dictionary<Guid, SectionResponseDto> MapStronglyTypedEmployeeSectionResponsesToDto(
        Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>> sectionResponses)
    {
        var result = new Dictionary<Guid, SectionResponseDto>();
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            Converters = { new QuestionResponseValueJsonConverter() }
        };

        foreach (var sectionKvp in sectionResponses)
        {
            var sectionId = sectionKvp.Key;
            var roleBasedResponses = sectionKvp.Value;

            var roleResponsesDto = new Dictionary<ResponseRole, Dictionary<Guid, QuestionResponseDto>>();

            // For employee endpoints, return EMPLOYEE responses only
            if (roleBasedResponses.TryGetValue(CompletionRole.Employee, out var employeeResponse))
            {
                var questionResponsesForSection = new Dictionary<Guid, QuestionResponseDto>();

                questionResponsesForSection[sectionId] = new QuestionResponseDto
                {
                    QuestionId = sectionId,
                    QuestionType = QuestionResponseMapper.InferQuestionType(employeeResponse),
                    ResponseData = QuestionResponseMapper.MapToDto(employeeResponse)
                };

                roleResponsesDto[ResponseRole.Employee] = questionResponsesForSection;
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
    /// - Post-review states (ReviewFinished onwards): Everyone sees ALL sections with ALL responses
    /// </summary>
    private static QuestionnaireResponseDto FilterSectionsByUserRoleAndWorkflowState(
        QuestionnaireResponseDto response,
        Application.Query.Queries.QuestionnaireAssignmentQueries.QuestionnaireAssignment assignment,
        Application.Query.Queries.QuestionnaireTemplateQueries.QuestionnaireTemplate template,
        Application.Query.Models.ApplicationRole userRole)
    {
        var isManager = userRole is Application.Query.Models.ApplicationRole.TeamLead or Application.Query.Models.ApplicationRole.HR or Application.Query.Models.ApplicationRole.HRLead or Application.Query.Models.ApplicationRole.Admin;

        // From post-review states onwards: Everyone sees ALL sections with ALL responses
        if (assignment.WorkflowState is WorkflowState.ReviewFinished or WorkflowState.EmployeeReviewConfirmed or WorkflowState.Finalized)
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
                    SectionId = sectionId,
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