using Microsoft.AspNetCore.Authorization;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.QueryApi.Authorization;
using ti8m.BeachBreak.QueryApi.Dto;
using ti8m.BeachBreak.QueryApi.Mappers;

namespace ti8m.BeachBreak.QueryApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for employee queries.
/// </summary>
public static class EmployeesEndpoints
{
    /// <summary>
    /// Maps employee query endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapEmployeesEndpoints(this WebApplication app)
    {
        var employeesGroup = app.MapGroup("/q/api/v{version:apiVersion}/employees")
            .WithTags("Employees")
            .RequireAuthorization(); // All endpoints require authentication

        // Get all employees with filtering and role restrictions
        employeesGroup.MapGet("/", async (
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            IEmployeeRoleService employeeRoleService,
            ILogger logger,
            bool includeDeleted = false,
            int? organizationNumber = null,
            string? role = null,
            Guid? managerId = null,
            CancellationToken cancellationToken = default) =>
        {
            logger.LogInformation("Received GetAllEmployees request - IncludeDeleted: {IncludeDeleted}, OrganizationNumber: {OrganizationNumber}, Role: {Role}, ManagerId: {ManagerId}",
                includeDeleted, organizationNumber, role, managerId);

            try
            {
                // Get current user ID for authorization
                Guid userId;
                try
                {
                    userId = await authorizationService.GetCurrentManagerIdAsync();
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.LogWarning("GetAllEmployees authorization failed: {Message}", ex.Message);
                    return Results.Unauthorized();
                }

                var hasElevatedRole = await HasElevatedRoleAsync(employeeRoleService, userId, logger);

                // If user is TeamLead (not HR/Admin), restrict to their direct reports
                Guid? effectiveManagerId = managerId;
                if (!hasElevatedRole)
                {
                    // TeamLead can only see their own team
                    effectiveManagerId = userId;
                    logger.LogInformation("TeamLead {UserId} requesting employees - restricting to their direct reports", userId);
                }

                var query = new EmployeeListQuery
                {
                    IncludeDeleted = includeDeleted,
                    OrganizationNumber = organizationNumber,
                    Role = role,
                    ManagerId = effectiveManagerId
                };

                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result.Succeeded && result.Payload != null)
                {
                    var employeeCount = result.Payload.Count();
                    logger.LogInformation("GetAllEmployees completed successfully, returned {EmployeeCount} employees", employeeCount);

                    var employees = result.Payload.Select(employee => new EmployeeDto
                    {
                        Id = employee.Id,
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        Role = employee.Role,
                        EMail = employee.EMail,
                        StartDate = employee.StartDate,
                        EndDate = employee.EndDate,
                        LastStartDate = employee.LastStartDate,
                        ManagerId = employee.ManagerId,
                        Manager = employee.Manager,
                        LoginName = employee.LoginName,
                        EmployeeNumber = employee.EmployeeNumber,
                        OrganizationNumber = employee.OrganizationNumber,
                        Organization = employee.Organization,
                        IsDeleted = employee.IsDeleted,
                        ApplicationRole = employee.ApplicationRole
                    });

                    return Results.Ok(employees);
                }
                else if (!result.Succeeded)
                {
                    logger.LogWarning("GetAllEmployees failed: {ErrorMessage}", result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }

                return Results.Ok(Enumerable.Empty<EmployeeDto>());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving employees");
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving employees",
                    statusCode: 500);
            }
        })
        .WithName("GetAllEmployees")
        .WithSummary("Get all employees")
        .WithDescription("Gets all employees with filtering - TeamLead users see only their direct reports, HR/Admin see all employees")
        .Produces<IEnumerable<EmployeeDto>>(200)
        .Produces(500);

        // Get employee by ID with authorization restrictions
        employeesGroup.MapGet("/{id:guid}", async (
            Guid id,
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            IEmployeeRoleService employeeRoleService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            logger.LogInformation("Received GetEmployee request for EmployeeId: {EmployeeId}", id);

            try
            {
                // Get current user ID for authorization
                Guid userId;
                try
                {
                    userId = await authorizationService.GetCurrentManagerIdAsync();
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.LogWarning("GetEmployee authorization failed: {Message}", ex.Message);
                    return Results.Unauthorized();
                }

                // Check authorization
                var hasElevatedRole = await HasElevatedRoleAsync(employeeRoleService, userId, logger);
                var isSelf = userId == id;
                var isDirectReport = !isSelf && !hasElevatedRole && await authorizationService.IsManagerOfAsync(userId, id);

                // Allow if: viewing self, has elevated role, or is manager of this employee
                if (!isSelf && !hasElevatedRole && !isDirectReport)
                {
                    logger.LogWarning("User {UserId} attempted to access employee {EmployeeId} without authorization", userId, id);
                    return Results.Forbid();
                }

                var result = await queryDispatcher.QueryAsync(new EmployeeQuery(id), cancellationToken);

                if (result?.Payload == null)
                {
                    logger.LogInformation("Employee not found for EmployeeId: {EmployeeId}", id);
                    return Results.NotFound($"Employee with ID {id} not found");
                }

                if (result.Succeeded)
                {
                    logger.LogInformation("GetEmployee completed successfully for EmployeeId: {EmployeeId}", id);

                    var employee = new EmployeeDto
                    {
                        Id = id,
                        FirstName = result.Payload.FirstName,
                        LastName = result.Payload.LastName,
                        Role = result.Payload.Role,
                        EMail = result.Payload.EMail,
                        StartDate = result.Payload.StartDate,
                        EndDate = result.Payload.EndDate,
                        LastStartDate = result.Payload.LastStartDate,
                        ManagerId = result.Payload.ManagerId,
                        Manager = result.Payload.Manager,
                        LoginName = result.Payload.LoginName,
                        EmployeeNumber = result.Payload.EmployeeNumber,
                        OrganizationNumber = result.Payload.OrganizationNumber,
                        Organization = result.Payload.Organization,
                        IsDeleted = result.Payload.IsDeleted,
                        ApplicationRole = result.Payload.ApplicationRole
                    };

                    return Results.Ok(employee);
                }
                else
                {
                    logger.LogWarning("GetEmployee failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}", id, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving employee {EmployeeId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving the employee",
                    statusCode: 500);
            }
        })
        .WithName("GetEmployee")
        .WithSummary("Get employee by ID")
        .WithDescription("Gets a specific employee by ID - users can view themselves, their direct reports (if manager), or any employee (if HR/Admin)")
        .Produces<EmployeeDto>(200)
        .Produces(404)
        .Produces(403)
        .Produces(500);

        // Get my assignments - authenticated user self-service
        employeesGroup.MapGet("/me/assignments", async (
            IQueryDispatcher queryDispatcher,
            UserContext userContext,
            ILogger logger,
            string? workflowState = null,
            CancellationToken cancellationToken = default) =>
        {
            // Get employee ID from authenticated user context (security best practice)
            if (!Guid.TryParse(userContext.Id, out var employeeId))
            {
                logger.LogWarning("GetMyAssignments failed: Unable to parse user ID from context");
                return Results.Unauthorized();
            }

            logger.LogInformation("Received GetMyAssignments request for authenticated EmployeeId: {EmployeeId}, WorkflowState: {WorkflowState}",
                employeeId, workflowState);

            try
            {
                var result = await queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId), cancellationToken);

                if (result.Succeeded)
                {
                    var assignmentCount = result.Payload?.Count() ?? 0;
                    logger.LogInformation("GetMyAssignments completed successfully for EmployeeId: {EmployeeId}, returned {AssignmentCount} assignments",
                        employeeId, assignmentCount);

                    // Filter by workflow state if provided
                    var filteredAssignments = result.Payload;
                    if (!string.IsNullOrWhiteSpace(workflowState) &&
                        Enum.TryParse<Domain.QuestionnaireAssignmentAggregate.WorkflowState>(workflowState, true, out var parsedState))
                    {
                        filteredAssignments = result.Payload.Where(a => a.WorkflowState == parsedState);
                    }

                    var assignments = filteredAssignments.Select(assignment => MapToQuestionnaireAssignmentDto(assignment));
                    return Results.Ok(assignments);
                }
                else
                {
                    logger.LogWarning("GetMyAssignments failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}", employeeId, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving assignments for authenticated employee {EmployeeId}", employeeId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving your assignments",
                    statusCode: 500);
            }
        })
        .WithName("GetMyAssignments")
        .WithSummary("Get my assignments")
        .WithDescription("Gets all questionnaire assignments for the currently authenticated employee - supports workflow state filtering")
        .Produces<IEnumerable<QuestionnaireAssignmentDto>>(200)
        .Produces(401)
        .Produces(403)
        .Produces(500);

        // Get my dashboard - authenticated user self-service
        employeesGroup.MapGet("/me/dashboard", async (
            IQueryDispatcher queryDispatcher,
            UserContext userContext,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            // Get employee ID from authenticated user context (security best practice)
            if (!Guid.TryParse(userContext.Id, out var employeeId))
            {
                logger.LogWarning("GetMyDashboard failed: Unable to parse user ID from context");
                return Results.Unauthorized();
            }

            logger.LogInformation("Received GetMyDashboard request for authenticated EmployeeId: {EmployeeId}", employeeId);

            try
            {
                var result = await queryDispatcher.QueryAsync(new EmployeeDashboardQuery(employeeId), cancellationToken);

                if (result?.Payload == null)
                {
                    logger.LogInformation("Dashboard not found for EmployeeId: {EmployeeId} - this is expected for new employees with no assignments", employeeId);

                    // Return empty dashboard for employees with no assignments yet
                    return Results.Ok(new EmployeeDashboardDto
                    {
                        EmployeeId = employeeId,
                        EmployeeFullName = string.Empty,
                        EmployeeEmail = string.Empty,
                        PendingCount = 0,
                        InProgressCount = 0,
                        CompletedCount = 0,
                        UrgentAssignments = new List<UrgentAssignmentDto>(),
                        LastUpdated = DateTime.UtcNow
                    });
                }

                if (result.Succeeded)
                {
                    logger.LogInformation("GetMyDashboard completed successfully for EmployeeId: {EmployeeId}", employeeId);

                    var dashboard = new EmployeeDashboardDto
                    {
                        EmployeeId = result.Payload.EmployeeId,
                        EmployeeFullName = result.Payload.EmployeeFullName,
                        EmployeeEmail = result.Payload.EmployeeEmail,
                        PendingCount = result.Payload.PendingCount,
                        InProgressCount = result.Payload.InProgressCount,
                        CompletedCount = result.Payload.CompletedCount,
                        UrgentAssignments = result.Payload.UrgentAssignments.Select(ua => new UrgentAssignmentDto
                        {
                            AssignmentId = ua.AssignmentId,
                            QuestionnaireTemplateName = ua.QuestionnaireTemplateName,
                            DueDate = ua.DueDate,
                            WorkflowState = ua.WorkflowState,
                            IsOverdue = ua.IsOverdue
                        }).ToList(),
                        LastUpdated = result.Payload.LastUpdated
                    };

                    return Results.Ok(dashboard);
                }
                else
                {
                    logger.LogWarning("GetMyDashboard failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}", employeeId, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving dashboard for authenticated employee {EmployeeId}", employeeId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving your dashboard",
                    statusCode: 500);
            }
        })
        .WithName("GetMyDashboard")
        .WithSummary("Get my dashboard")
        .WithDescription("Gets the dashboard metrics for the currently authenticated employee - returns assignment counts and urgent assignments list")
        .Produces<EmployeeDashboardDto>(200)
        .Produces(401)
        .Produces(404)
        .Produces(500);

        // Get my assignment by ID - authenticated user self-service
        employeesGroup.MapGet("/me/assignments/{assignmentId:guid}", async (
            Guid assignmentId,
            IQueryDispatcher queryDispatcher,
            UserContext userContext,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            // Get employee ID from authenticated user context (security best practice)
            if (!Guid.TryParse(userContext.Id, out var employeeId))
            {
                logger.LogWarning("GetMyAssignmentById failed: Unable to parse user ID from context");
                return Results.Unauthorized();
            }

            logger.LogInformation("Received GetMyAssignmentById request for authenticated EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                employeeId, assignmentId);

            try
            {
                // Query all assignments for this employee
                var result = await queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId), cancellationToken);

                if (!result.Succeeded)
                {
                    logger.LogWarning("GetMyAssignmentById failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}",
                        employeeId, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }

                // Find the specific assignment
                var assignment = result.Payload?.FirstOrDefault(a => a.Id == assignmentId);

                if (assignment == null)
                {
                    logger.LogWarning("Assignment {AssignmentId} not found for EmployeeId: {EmployeeId}", assignmentId, employeeId);
                    return Results.NotFound($"Assignment {assignmentId} not found or does not belong to you");
                }

                logger.LogInformation("GetMyAssignmentById completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                    employeeId, assignmentId);

                var dto = MapToQuestionnaireAssignmentDto(assignment);
                return Results.Ok(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving assignment {AssignmentId} for authenticated employee {EmployeeId}",
                    assignmentId, employeeId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving your assignment",
                    statusCode: 500);
            }
        })
        .WithName("GetMyAssignmentById")
        .WithSummary("Get my assignment by ID")
        .WithDescription("Gets a specific questionnaire assignment by ID for the currently authenticated employee")
        .Produces<QuestionnaireAssignmentDto>(200)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(500);

        // Get my assignment custom sections - authenticated user self-service
        employeesGroup.MapGet("/me/assignments/{assignmentId:guid}/custom-sections", async (
            Guid assignmentId,
            IQueryDispatcher queryDispatcher,
            UserContext userContext,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            // Get employee ID from authenticated user context
            if (!Guid.TryParse(userContext.Id, out var employeeId))
            {
                logger.LogWarning("GetMyAssignmentCustomSections failed: Unable to parse user ID from context");
                return Results.Unauthorized();
            }

            logger.LogInformation("Received GetMyAssignmentCustomSections request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                employeeId, assignmentId);

            try
            {
                // First verify the assignment belongs to this employee
                var assignmentListResult = await queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId), cancellationToken);

                if (!assignmentListResult.Succeeded)
                {
                    logger.LogWarning("GetMyAssignmentCustomSections failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}",
                        employeeId, assignmentListResult.Message);
                    return Results.Problem(detail: assignmentListResult.Message, statusCode: assignmentListResult.StatusCode);
                }

                // Check if assignment exists and belongs to employee
                var assignment = assignmentListResult.Payload?.FirstOrDefault(a => a.Id == assignmentId);
                if (assignment == null)
                {
                    logger.LogWarning("Assignment {AssignmentId} not found for EmployeeId: {EmployeeId}", assignmentId, employeeId);
                    return Results.NotFound($"Assignment {assignmentId} not found or does not belong to you");
                }

                // Get custom sections and map to DTOs with enums (same pattern as templates and manager endpoint)
                var customSectionsQuery = new GetAssignmentCustomSectionsQuery(assignmentId);
                var result = await queryDispatcher.QueryAsync(customSectionsQuery, cancellationToken);

                if (result.Succeeded)
                {
                    logger.LogInformation("GetMyAssignmentCustomSections completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                        employeeId, assignmentId);

                    var sections = result.Payload.Select(MapSectionToDto);
                    return Results.Ok(sections);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving custom sections for assignment {AssignmentId} for employee {EmployeeId}",
                    assignmentId, employeeId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving custom sections",
                    statusCode: 500);
            }
        })
        .WithName("GetMyAssignmentCustomSections")
        .WithSummary("Get my assignment custom sections")
        .WithDescription("Gets custom sections for a specific assignment for the currently authenticated employee - returns instance-specific sections that were added during manager initialization")
        .Produces<IEnumerable<QuestionSectionDto>>(200)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(500);

        // Get my response by assignment - authenticated user self-service
        employeesGroup.MapGet("/me/responses/assignment/{assignmentId:guid}", async (
            Guid assignmentId,
            IQueryDispatcher queryDispatcher,
            UserContext userContext,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            // Get employee ID from authenticated user context (security best practice)
            if (!Guid.TryParse(userContext.Id, out var employeeId))
            {
                logger.LogWarning("GetMyResponse failed: Unable to parse user ID from context");
                return Results.Unauthorized();
            }

            logger.LogInformation("Received GetMyResponse request for authenticated EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                employeeId, assignmentId);

            try
            {
                var query = new Application.Query.Queries.ResponseQueries.GetResponseByAssignmentIdQuery(assignmentId);
                var response = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (response == null)
                {
                    logger.LogInformation("Response not found for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);
                    return Results.NotFound($"Response not found for assignment {assignmentId}");
                }

                // Verify this response belongs to the requesting employee
                if (response.EmployeeId != employeeId)
                {
                    logger.LogWarning("Employee {EmployeeId} attempted to access response for Assignment {AssignmentId} belonging to {ActualEmployeeId}",
                        employeeId, assignmentId, response.EmployeeId);
                    return Results.Forbid();
                }

                logger.LogInformation("GetMyResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                    employeeId, assignmentId);

                // Map section responses
                // Note: Response structure is Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>>
                // For employee "me" endpoint, we only return EMPLOYEE responses
                var sectionResponsesDto = new Dictionary<Guid, SectionResponseDto>();
                foreach (var sectionKvp in response.SectionResponses)
                {
                    var sectionId = sectionKvp.Key;
                    var roleBasedResponses = sectionKvp.Value;

                    var questionResponsesDict = new Dictionary<Guid, QuestionResponseDto>();

                    // Only extract Employee role responses for this endpoint
                    if (roleBasedResponses.TryGetValue(CompletionRole.Employee, out var employeeResponse))
                    {
                        var questionResponseDto = new QuestionResponseDto
                        {
                            QuestionId = sectionId,
                            ResponseData = QuestionResponseMapper.MapToDto(employeeResponse),
                            QuestionType = QuestionResponseMapper.InferQuestionType(employeeResponse)
                        };

                        questionResponsesDict[sectionId] = questionResponseDto;
                    }

                    // Only include sections that have employee responses
                    if (questionResponsesDict.Any())
                    {
                        sectionResponsesDto[sectionId] = new SectionResponseDto
                        {
                            SectionId = sectionId,
                            RoleResponses = new Dictionary<ResponseRole, Dictionary<Guid, QuestionResponseDto>>
                            {
                                { ResponseRole.Employee, questionResponsesDict }
                            }
                        };
                    }
                }

                var dto = new QuestionnaireResponseDto
                {
                    Id = response.Id,
                    AssignmentId = response.AssignmentId,
                    TemplateId = response.TemplateId,
                    EmployeeId = response.EmployeeId.ToString(),
                    SectionResponses = sectionResponsesDto,
                    StartedDate = response.StartedDate,
                    ProgressPercentage = response.ProgressPercentage
                };

                return Results.Ok(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving response for assignment {AssignmentId} and employee {EmployeeId}",
                    assignmentId, employeeId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving your response",
                    statusCode: 500);
            }
        })
        .WithName("GetMyResponse")
        .WithSummary("Get my response")
        .WithDescription("Gets the questionnaire response for a specific assignment for the currently authenticated employee")
        .Produces<QuestionnaireResponseDto>(200)
        .Produces(401)
        .Produces(404)
        .Produces(500);

        // Get my goal question data - authenticated user self-service
        employeesGroup.MapGet("/me/assignments/{assignmentId:guid}/goals/{questionId:guid}", async (
            Guid assignmentId,
            Guid questionId,
            IQueryDispatcher queryDispatcher,
            UserContext userContext,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                // Get current user ID from UserContext
                if (!Guid.TryParse(userContext.Id, out var userId))
                {
                    logger.LogWarning("Failed to parse user ID from context");
                    return Results.Unauthorized();
                }

                // Verify the assignment belongs to the current employee
                var assignmentResult = await queryDispatcher.QueryAsync(
                    new QuestionnaireEmployeeAssignmentListQuery(userId), cancellationToken);

                if (assignmentResult?.Payload == null || !assignmentResult.Payload.Any(a => a.Id == assignmentId))
                {
                    logger.LogWarning("Employee {UserId} attempted to access assignment {AssignmentId} that doesn't belong to them",
                        userId, assignmentId);
                    return Results.NotFound($"Assignment {assignmentId} not found or does not belong to you");
                }

                // Employees always see goals with Employee role filter
                var query = new Application.Query.Queries.QuestionnaireAssignmentQueries.GetGoalQuestionDataQuery(
                    assignmentId, questionId, ApplicationRoleMapper.MapFromDomain(Domain.EmployeeAggregate.ApplicationRole.Employee));

                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result == null)
                {
                    logger.LogWarning("Query returned null for assignment {AssignmentId}, question {QuestionId}",
                        assignmentId, questionId);
                    return Results.NotFound();
                }

                if (!result.Succeeded)
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }

                logger.LogInformation("Returning goal data for employee {UserId}: Assignment {AssignmentId}, Question {QuestionId}, WorkflowState: {WorkflowState}, Goals Count: {GoalCount}",
                    userId, assignmentId, questionId,
                    result.Payload?.WorkflowState,
                    result.Payload?.Goals?.Count ?? 0);

                if (result.Payload?.Goals != null)
                {
                    foreach (var goal in result.Payload.Goals)
                    {
                        logger.LogInformation("  Goal {GoalId}: AddedByRole={AddedByRole}", goal.Id, goal.AddedByRole);
                    }
                }

                return Results.Ok(result.Payload);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting goal data for assignment {AssignmentId}, question {QuestionId}",
                    assignmentId, questionId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving goal data",
                    statusCode: 500);
            }
        })
        .WithName("GetMyGoalQuestionData")
        .WithSummary("Get my goal question data")
        .WithDescription("Gets all goal data for a specific question within an assignment for the current employee - includes goals added by Employee and ratings of predecessor goals")
        .Produces<GoalQuestionDataDto>(200)
        .Produces(401)
        .Produces(404)
        .Produces(500);

        // Get assignments for specific employee - TeamLead+ with manager restrictions
        employeesGroup.MapGet("/{employeeId:guid}/assignments", async (
            Guid employeeId,
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            IEmployeeRoleService employeeRoleService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            logger.LogInformation("Received GetEmployeeAssignments request for EmployeeId: {EmployeeId}", employeeId);

            try
            {
                // Get current user ID for authorization
                Guid userId;
                try
                {
                    userId = await authorizationService.GetCurrentManagerIdAsync();
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.LogWarning("GetEmployeeAssignments authorization failed: {Message}", ex.Message);
                    return Results.Unauthorized();
                }

                // Check authorization
                var hasElevatedRole = await HasElevatedRoleAsync(employeeRoleService, userId, logger);
                if (!hasElevatedRole)
                {
                    // TeamLead must be manager of this employee
                    var isDirectReport = await authorizationService.IsManagerOfAsync(userId, employeeId);
                    if (!isDirectReport)
                    {
                        logger.LogWarning("Manager {UserId} attempted to access assignments for non-direct report employee {EmployeeId}",
                            userId, employeeId);
                        return Results.Forbid();
                    }
                }

                var result = await queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId), cancellationToken);

                if (result.Succeeded)
                {
                    var assignmentCount = result.Payload?.Count() ?? 0;
                    logger.LogInformation("GetEmployeeAssignments completed successfully for EmployeeId: {EmployeeId}, returned {AssignmentCount} assignments",
                        employeeId, assignmentCount);

                    var assignments = result.Payload.Select(assignment => new QuestionnaireAssignmentDto
                    {
                        Id = assignment.Id,
                        EmployeeId = assignment.EmployeeId.ToString(),
                        EmployeeName = assignment.EmployeeName,
                        EmployeeEmail = assignment.EmployeeEmail,
                        TemplateId = assignment.TemplateId,
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
                else
                {
                    logger.LogWarning("GetEmployeeAssignments failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}", employeeId, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
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
        .RequireAuthorization("TeamLead")
        .WithName("GetEmployeeAssignments")
        .WithSummary("Get employee assignments")
        .WithDescription("Gets all questionnaire assignments for a specific employee - managers can only view assignments for their direct reports, HR/Admin can view assignments for any employee")
        .Produces<IEnumerable<QuestionnaireAssignmentDto>>(200)
        .Produces(403)
        .Produces(500);

        // Get employee language preference with authorization restrictions
        employeesGroup.MapGet("/{id:guid}/language", async (
            Guid id,
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            IEmployeeRoleService employeeRoleService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            logger.LogInformation("Received GetEmployeeLanguage request for EmployeeId: {EmployeeId}", id);

            try
            {
                // Get current user ID for authorization
                Guid userId;
                try
                {
                    userId = await authorizationService.GetCurrentManagerIdAsync();
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.LogWarning("GetEmployeeLanguage authorization failed: {Message}", ex.Message);
                    return Results.Unauthorized();
                }

                // Check authorization - users can only access their own language or have elevated role
                var hasElevatedRole = await HasElevatedRoleAsync(employeeRoleService, userId, logger);
                var isSelf = userId == id;

                if (!isSelf && !hasElevatedRole)
                {
                    logger.LogWarning("User {UserId} attempted to access language for employee {EmployeeId} without authorization", userId, id);
                    return Results.Forbid();
                }

                var result = await queryDispatcher.QueryAsync(new EmployeeQuery(id), cancellationToken);

                if (result?.Payload == null)
                {
                    logger.LogInformation("Employee not found for EmployeeId: {EmployeeId}", id);
                    return Results.NotFound($"Employee with ID {id} not found");
                }

                if (!result.Succeeded)
                {
                    logger.LogWarning("GetEmployeeLanguage failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}", id, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }

                logger.LogInformation("GetEmployeeLanguage completed successfully for EmployeeId: {EmployeeId}, Language: {Language}",
                    id, result.Payload.PreferredLanguage);

                // Return language as JSON string
                return Results.Ok(result.Payload.PreferredLanguage.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving language for employee {EmployeeId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving the employee language",
                    statusCode: 500);
            }
        })
        .WithName("GetEmployeeLanguage")
        .WithSummary("Get employee language preference")
        .WithDescription("Gets the preferred language for a specific employee - users can only access their own language preference unless they have elevated roles")
        .Produces<string>(200)
        .Produces(404)
        .Produces(403)
        .Produces(500);
    }

    /// <summary>
    /// Checks if the current user has an elevated role (HR, HRLead, or Admin).
    /// Returns true if elevated, false if user is only TeamLead/Employee.
    /// </summary>
    private static async Task<bool> HasElevatedRoleAsync(IEmployeeRoleService employeeRoleService, Guid userId, ILogger logger, CancellationToken cancellationToken = default)
    {
        var employeeRole = await employeeRoleService.GetEmployeeRoleAsync(userId, cancellationToken);
        if (employeeRole == null)
        {
            logger.LogWarning("Unable to retrieve employee role for user {UserId}", userId);
            return false;
        }

        // EmployeeRoleResult.ApplicationRole is already Application.Query.ApplicationRole
        return employeeRole.ApplicationRole == Application.Query.Models.ApplicationRole.HR ||
               employeeRole.ApplicationRole == Application.Query.Models.ApplicationRole.HRLead ||
               employeeRole.ApplicationRole == Application.Query.Models.ApplicationRole.Admin;
    }

    /// <summary>
    /// Maps a QuestionnaireAssignment query result to a QuestionnaireAssignmentDto.
    /// Includes all workflow properties for proper state management on the frontend.
    /// </summary>
    private static QuestionnaireAssignmentDto MapToQuestionnaireAssignmentDto(Application.Query.Queries.QuestionnaireAssignmentQueries.QuestionnaireAssignment assignment)
    {
        return new QuestionnaireAssignmentDto
        {
            Id = assignment.Id,
            EmployeeId = assignment.EmployeeId.ToString(),
            EmployeeName = assignment.EmployeeName,
            EmployeeEmail = assignment.EmployeeEmail,
            TemplateId = assignment.TemplateId,
            ProcessType = MapProcessType(assignment.ProcessType),
            TemplateName = assignment.TemplateName,
            TemplateCategoryId = assignment.TemplateCategoryId,
            WorkflowState = assignment.WorkflowState,
            SectionProgress = assignment.SectionProgress,
            AssignedDate = assignment.AssignedDate,
            DueDate = assignment.DueDate,
            StartedDate = assignment.StartedDate,
            CompletedDate = assignment.CompletedDate,
            AssignedBy = assignment.AssignedBy,
            Notes = assignment.Notes,

            // Withdrawal tracking
            IsWithdrawn = assignment.IsWithdrawn,
            WithdrawnDate = assignment.WithdrawnDate,
            WithdrawnByEmployeeId = assignment.WithdrawnByEmployeeId,
            WithdrawnByEmployeeName = assignment.WithdrawnByEmployeeName,
            WithdrawalReason = assignment.WithdrawalReason,

            // Submission phase
            EmployeeSubmittedDate = assignment.EmployeeSubmittedDate,
            EmployeeSubmittedByEmployeeId = assignment.EmployeeSubmittedByEmployeeId,
            EmployeeSubmittedByEmployeeName = assignment.EmployeeSubmittedByEmployeeName,
            ManagerSubmittedDate = assignment.ManagerSubmittedDate,
            ManagerSubmittedByEmployeeId = assignment.ManagerSubmittedByEmployeeId,
            ManagerSubmittedByEmployeeName = assignment.ManagerSubmittedByEmployeeName,

            // Review phase
            ReviewInitiatedDate = assignment.ReviewInitiatedDate,
            ReviewInitiatedByEmployeeId = assignment.ReviewInitiatedByEmployeeId,
            ReviewInitiatedByEmployeeName = assignment.ReviewInitiatedByEmployeeName,
            ManagerReviewFinishedDate = assignment.ManagerReviewFinishedDate,
            ManagerReviewFinishedByEmployeeId = assignment.ManagerReviewFinishedByEmployeeId,
            ManagerReviewFinishedByEmployeeName = assignment.ManagerReviewFinishedByEmployeeName,
            ManagerReviewSummary = assignment.ManagerReviewSummary,
            EmployeeReviewConfirmedDate = assignment.EmployeeReviewConfirmedDate,
            EmployeeReviewConfirmedByEmployeeId = assignment.EmployeeReviewConfirmedByEmployeeId,
            EmployeeReviewConfirmedByEmployeeName = assignment.EmployeeReviewConfirmedByEmployeeName,
            EmployeeReviewComments = assignment.EmployeeReviewComments,

            // Final state
            FinalizedDate = assignment.FinalizedDate,
            FinalizedByEmployeeId = assignment.FinalizedByEmployeeId,
            FinalizedByEmployeeName = assignment.FinalizedByEmployeeName,
            ManagerFinalNotes = assignment.ManagerFinalNotes,
            IsLocked = assignment.IsLocked,

            // Reopen tracking
            LastReopenedDate = assignment.LastReopenedDate,
            LastReopenedByEmployeeId = assignment.LastReopenedByEmployeeId,
            LastReopenedByEmployeeName = assignment.LastReopenedByEmployeeName,
            LastReopenedByRole = assignment.LastReopenedByRole,
            LastReopenReason = assignment.LastReopenReason,

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
        };
    }

    /// <summary>
    /// Maps query-side QuestionSection (strings) to DTO (enums) for client consumption.
    /// Same pattern used by QuestionnaireTemplatesController and AssignmentsController for consistency.
    /// </summary>
    private static QuestionSectionDto MapSectionToDto(Application.Query.Queries.QuestionnaireTemplateQueries.QuestionSection section)
    {
        return new QuestionSectionDto
        {
            Id = section.Id,
            TitleGerman = section.TitleGerman,
            TitleEnglish = section.TitleEnglish,
            DescriptionGerman = section.DescriptionGerman,
            DescriptionEnglish = section.DescriptionEnglish,
            Order = section.Order,
            CompletionRole = MapToCompletionRoleEnum(section.CompletionRole),
            Type = MapQuestionTypeFromString(section.Type),
            Configuration = section.Configuration,
            IsInstanceSpecific = section.IsInstanceSpecific
        };
    }

    private static CompletionRole MapToCompletionRoleEnum(string completionRole)
    {
        return completionRole?.ToLower() switch
        {
            "manager" => CompletionRole.Manager,
            "both" => CompletionRole.Both,
            _ => CompletionRole.Employee
        };
    }

    private static QueryApi.Dto.QuestionType MapQuestionTypeFromString(string type)
    {
        return type?.ToLower() switch
        {
            "textquestion" => QueryApi.Dto.QuestionType.TextQuestion,
            "goal" => QueryApi.Dto.QuestionType.Goal,
            "assessment" => QueryApi.Dto.QuestionType.Assessment,
            _ => QueryApi.Dto.QuestionType.Assessment
        };
    }

    private static QueryApi.Dto.QuestionnaireProcessType MapProcessType(Core.Domain.QuestionnaireProcessType domainProcessType)
    {
        return domainProcessType switch
        {
            Core.Domain.QuestionnaireProcessType.PerformanceReview => QueryApi.Dto.QuestionnaireProcessType.PerformanceReview,
            Core.Domain.QuestionnaireProcessType.Survey => QueryApi.Dto.QuestionnaireProcessType.Survey,
            _ => throw new ArgumentOutOfRangeException(nameof(domainProcessType), domainProcessType, "Unknown process type")
        };
    }
}