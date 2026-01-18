using Microsoft.AspNetCore.Authorization;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.QueryApi.Authorization;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for assignment queries.
/// </summary>
public static class AssignmentsEndpoints
{
    /// <summary>
    /// Maps assignment query endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapAssignmentsEndpoints(this WebApplication app)
    {
        var assignmentsGroup = app.MapGroup("/q/api/v{version:apiVersion}/assignments")
            .WithTags("Assignments")
            .RequireAuthorization(); // Base authorization - specific roles on individual endpoints

        // Get all assignments - HR only
        assignmentsGroup.MapGet("/", async (
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var result = await queryDispatcher.QueryAsync(new QuestionnaireAssignmentListQuery(), cancellationToken);

                if (result.Succeeded)
                {
                    var assignments = result.Payload.Select(MapToDto);
                    return Results.Ok(assignments);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving assignments");
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving assignments",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("HR")
        .WithName("GetAllAssignments")
        .WithSummary("Get all assignments")
        .WithDescription("Gets all assignments - HR/Admin only")
        .Produces<IEnumerable<QuestionnaireAssignmentDto>>(200)
        .Produces(500);

        // Get assignment by ID - TeamLead+ with manager restrictions
        assignmentsGroup.MapGet("/{id:guid}", async (
            Guid id,
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            IEmployeeRoleService employeeRoleService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                // Get the assignment first
                var result = await queryDispatcher.QueryAsync(new QuestionnaireAssignmentQuery(id), cancellationToken);
                if (result == null || !result.Succeeded || result.Payload == null)
                    return Results.NotFound($"Assignment with ID {id} not found");

                var assignment = result.Payload;

                // Get current user ID
                Guid userId;
                try
                {
                    userId = await authorizationService.GetCurrentManagerIdAsync();
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.LogWarning("GetAssignment authorization failed: {Message}", ex.Message);
                    return Results.Unauthorized();
                }

                // Check if user has elevated role (HR/Admin) - they can access any assignment
                var hasElevatedRole = await HasElevatedRoleAsync(employeeRoleService, userId, logger);
                if (!hasElevatedRole)
                {
                    // Managers can only access assignments for their direct reports
                    var canAccess = await authorizationService.CanAccessAssignmentAsync(userId, id);
                    if (!canAccess)
                    {
                        logger.LogWarning("Manager {UserId} attempted to access assignment {AssignmentId} for non-direct report",
                            userId, id);
                        return Results.Forbid();
                    }
                }

                return Results.Ok(MapToDto(assignment));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving assignment {AssignmentId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving the assignment",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("TeamLead")
        .WithName("GetAssignment")
        .WithSummary("Get assignment by ID")
        .WithDescription("Gets a specific assignment by ID - managers can only view assignments for their direct reports, HR/Admin can view any assignment")
        .Produces<QuestionnaireAssignmentDto>(200)
        .Produces(403)
        .Produces(404)
        .Produces(500);

        // Get assignments by employee - TeamLead+ with manager restrictions
        assignmentsGroup.MapGet("/employee/{employeeId}", async (
            Guid employeeId,
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            IEmployeeRoleService employeeRoleService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                // Check authorization - only apply manager restrictions if user doesn't have elevated HR/Admin roles
                Guid userId;
                try
                {
                    userId = await authorizationService.GetCurrentManagerIdAsync();
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.LogWarning("GetAssignmentsByEmployee authorization failed: {Message}", ex.Message);
                    return Results.Unauthorized();
                }

                var hasElevatedRole = await HasElevatedRoleAsync(employeeRoleService, userId, logger);
                if (!hasElevatedRole)
                {
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
                    var assignments = result.Payload.Select(MapToDto);
                    return Results.Ok(assignments);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", employeeId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving assignments",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("TeamLead")
        .WithName("GetAssignmentsByEmployee")
        .WithSummary("Get assignments by employee")
        .WithDescription("Gets all assignments for a specific employee - managers can only view assignments for their direct reports, HR/Admin can view assignments for any employee")
        .Produces<IEnumerable<QuestionnaireAssignmentDto>>(200)
        .Produces(403)
        .Produces(500);

        // Get review changes for assignment - TeamLead+ with manager restrictions
        assignmentsGroup.MapGet("/{id:guid}/review-changes", async (
            Guid id,
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            IEmployeeRoleService employeeRoleService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                // Check authorization - only apply manager restrictions if user doesn't have elevated HR/Admin roles
                Guid userId;
                try
                {
                    userId = await authorizationService.GetCurrentManagerIdAsync();
                }
                catch (UnauthorizedAccessException ex)
                {
                    logger.LogWarning("GetReviewChanges authorization failed: {Message}", ex.Message);
                    return Results.Unauthorized();
                }

                var hasElevatedRole = await HasElevatedRoleAsync(employeeRoleService, userId, logger);
                if (!hasElevatedRole)
                {
                    var canAccess = await authorizationService.CanAccessAssignmentAsync(userId, id);

                    if (!canAccess)
                    {
                        logger.LogWarning("Manager {UserId} attempted to access review changes for assignment {AssignmentId} for non-direct report",
                            userId, id);
                        return Results.Forbid();
                    }
                }

                var result = await queryDispatcher.QueryAsync(new Application.Query.Queries.ReviewQueries.GetReviewChangesQuery(id), cancellationToken);

                // Batch fetch employee names for all changes
                var employeeIds = result.Select(c => c.ChangedByEmployeeId).Distinct().ToList();
                var employeeNames = new Dictionary<Guid, string>();

                foreach (var employeeId in employeeIds)
                {
                    var employeeResult = await queryDispatcher.QueryAsync(new EmployeeQuery(employeeId), cancellationToken);
                    if (employeeResult?.Succeeded == true && employeeResult.Payload != null)
                    {
                        var employee = employeeResult.Payload;
                        employeeNames[employeeId] = $"{employee.FirstName} {employee.LastName}";
                    }
                    else
                    {
                        employeeNames[employeeId] = "Unknown";
                    }
                }

                var changes = result.Select(c => new ReviewChangeDto
                {
                    Id = c.Id,
                    AssignmentId = c.AssignmentId,
                    SectionId = c.SectionId,
                    SectionTitle = c.SectionTitle,
                    QuestionId = c.QuestionId,
                    QuestionTitle = c.QuestionTitle,
                    OriginalCompletionRole = c.OriginalCompletionRole,
                    OldValue = c.OldValue,
                    NewValue = c.NewValue,
                    ChangedAt = c.ChangedAt,
                    ChangedBy = employeeNames.TryGetValue(c.ChangedByEmployeeId, out var name) ? name : "Unknown"
                });

                return Results.Ok(changes);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving review changes for assignment {AssignmentId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving review changes",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("TeamLead")
        .WithName("GetReviewChanges")
        .WithSummary("Get review changes")
        .WithDescription("Gets all review changes for a specific assignment - returns a list of all edits made by the manager during the review meeting")
        .Produces<IEnumerable<ReviewChangeDto>>(200)
        .Produces(403)
        .Produces(404)
        .Produces(500);

        // Get custom sections for assignment - TeamLead+ with manager restrictions
        assignmentsGroup.MapGet("/{assignmentId}/custom-sections", async (
            Guid assignmentId,
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            IEmployeeRoleService employeeRoleService,
            UserContext userContext,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                // Get authenticated user ID
                if (!Guid.TryParse(userContext.Id, out var userId))
                {
                    logger.LogWarning("Failed to parse user ID from context");
                    return Results.Unauthorized();
                }

                // Check if user has elevated role (HR/Admin)
                var hasElevatedRole = await HasElevatedRoleAsync(employeeRoleService, userId, logger);
                if (!hasElevatedRole)
                {
                    // Managers can only access assignments for their direct reports
                    var canAccess = await authorizationService.CanAccessAssignmentAsync(userId, assignmentId);
                    if (!canAccess)
                    {
                        logger.LogWarning("Manager {UserId} attempted to access custom sections for assignment {AssignmentId} for non-direct report",
                            userId, assignmentId);
                        return Results.Forbid();
                    }
                }

                var query = new GetAssignmentCustomSectionsQuery(assignmentId);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result.Succeeded)
                {
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
                logger.LogError(ex, "Error retrieving custom sections for assignment {AssignmentId}", assignmentId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving custom sections",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("TeamLead")
        .WithName("GetCustomSections")
        .WithSummary("Get custom sections")
        .WithDescription("Gets all custom sections for a specific assignment - custom sections are instance-specific and added during initialization phase")
        .Produces<IEnumerable<QuestionSectionDto>>(200)
        .Produces(403)
        .Produces(404)
        .Produces(500);

        // Get available predecessors - TeamLead+ with manager restrictions
        assignmentsGroup.MapGet("/{assignmentId}/predecessors", async (
            Guid assignmentId,
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            IEmployeeRoleService employeeRoleService,
            UserContext userContext,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                // Get authenticated user ID
                if (!Guid.TryParse(userContext.Id, out var userId))
                {
                    logger.LogWarning("Failed to parse user ID from context");
                    return Results.Unauthorized();
                }

                // Get the assignment first to determine the employee
                var assignmentResult = await queryDispatcher.QueryAsync(new QuestionnaireAssignmentQuery(assignmentId), cancellationToken);
                if (assignmentResult == null || !assignmentResult.Succeeded || assignmentResult.Payload == null)
                {
                    logger.LogWarning("Assignment {AssignmentId} not found", assignmentId);
                    return Results.NotFound($"Assignment with ID {assignmentId} not found");
                }

                var assignment = assignmentResult.Payload;
                var employeeId = assignment.EmployeeId;

                // Check if user has elevated role (HR/Admin) or is the employee themselves
                var hasElevatedRole = await HasElevatedRoleAsync(employeeRoleService, userId, logger);
                if (!hasElevatedRole && userId != employeeId)
                {
                    // Managers can only access assignments for their direct reports
                    var canAccess = await authorizationService.CanAccessAssignmentAsync(userId, assignmentId);
                    if (!canAccess)
                    {
                        logger.LogWarning("Manager {UserId} attempted to access predecessors for assignment {AssignmentId} for non-direct report",
                            userId, assignmentId);
                        return Results.Forbid();
                    }
                }

                // Query uses the employee ID (not the requesting user ID) to get predecessors for the employee
                var query = new Application.Query.Queries.QuestionnaireAssignmentQueries.GetAvailablePredecessorsQuery(
                    assignmentId, employeeId);

                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result == null)
                {
                    logger.LogWarning("Query returned null for assignment {AssignmentId}", assignmentId);
                    return Results.NotFound();
                }

                if (!result.Succeeded)
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }

                return Results.Ok(result.Payload);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting available predecessors for assignment {AssignmentId}", assignmentId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving available predecessors",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("TeamLead")
        .WithName("GetAvailablePredecessors")
        .WithSummary("Get available predecessors")
        .WithDescription("Gets available predecessor questionnaires that can be linked for goal rating - returns finalized questionnaires for same employee, same category, that have ANY goals")
        .Produces<IEnumerable<AvailablePredecessorDto>>(200)
        .Produces(403)
        .Produces(404)
        .Produces(500);

        // Get goal question data - TeamLead+
        assignmentsGroup.MapGet("/{assignmentId:guid}/goals/{questionId:guid}", async (
            Guid assignmentId,
            Guid questionId,
            IQueryDispatcher queryDispatcher,
            IEmployeeRoleService employeeRoleService,
            UserContext userContext,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                // Get current user ID
                if (!Guid.TryParse(userContext.Id, out var userId))
                {
                    logger.LogWarning("Failed to parse user ID from context");
                    return Results.Unauthorized();
                }

                // Determine current user's role using the employee role service
                var employeeRole = await employeeRoleService.GetEmployeeRoleAsync(userId, cancellationToken);
                if (employeeRole == null)
                {
                    logger.LogWarning("Unable to retrieve employee role for user {UserId}", userId);
                    return Results.Unauthorized();
                }

                // Use ApplicationRole directly (no premature mapping)
                // The query handler will determine proper role-based filtering
                var query = new Application.Query.Queries.QuestionnaireAssignmentQueries.GetGoalQuestionDataQuery(
                    assignmentId, questionId, employeeRole.ApplicationRole);

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
        .RequireAuthorization("TeamLead")
        .WithName("GetGoalQuestionData")
        .WithSummary("Get goal question data")
        .WithDescription("Gets all goal data for a specific question within an assignment - includes goals added by Employee/Manager and ratings of predecessor goals")
        .Produces<GoalQuestionDataDto>(200)
        .Produces(401)
        .Produces(404)
        .Produces(500);

        // Get available employee feedback - all authenticated users
        assignmentsGroup.MapGet("/{assignmentId}/feedback/available", async (
            Guid assignmentId,
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var query = new Application.Query.Queries.QuestionnaireAssignmentQueries.GetAvailableEmployeeFeedbackQuery(assignmentId);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result == null)
                {
                    logger.LogWarning("Query returned null for assignment {AssignmentId}", assignmentId);
                    return Results.NotFound();
                }

                if (result.Succeeded)
                {
                    return Results.Ok(result.Payload);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting available feedback for assignment {AssignmentId}", assignmentId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving available feedback",
                    statusCode: 500);
            }
        })
        .WithName("GetAvailableEmployeeFeedback")
        .WithSummary("Get available employee feedback")
        .WithDescription("Gets all available employee feedback records that can be linked to this assignment - returns non-deleted feedback for the employee")
        .Produces<List<Application.Query.Projections.Models.LinkedEmployeeFeedbackDto>>(200)
        .Produces(404)
        .Produces(500);

        // Get feedback question data - all authenticated users
        assignmentsGroup.MapGet("/{assignmentId}/feedback/{questionId}", async (
            Guid assignmentId,
            Guid questionId,
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var query = new Application.Query.Queries.QuestionnaireAssignmentQueries.GetFeedbackQuestionDataQuery(
                    assignmentId, questionId);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result == null)
                {
                    logger.LogWarning("Query returned null for assignment {AssignmentId}, question {QuestionId}",
                        assignmentId, questionId);
                    return Results.NotFound();
                }

                if (result.Succeeded)
                {
                    return Results.Ok(result.Payload);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting feedback data for assignment {AssignmentId}, question {QuestionId}",
                    assignmentId, questionId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving feedback data",
                    statusCode: 500);
            }
        })
        .WithName("GetFeedbackQuestionData")
        .WithSummary("Get feedback question data")
        .WithDescription("Gets all linked feedback data for a specific question within an assignment - returns the full feedback details for all linked feedback records")
        .Produces<Application.Query.Projections.Models.FeedbackQuestionDataDto>(200)
        .Produces(404)
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
    private static QuestionnaireAssignmentDto MapToDto(Application.Query.Queries.QuestionnaireAssignmentQueries.QuestionnaireAssignment assignment)
    {
        return new QuestionnaireAssignmentDto
        {
            AssignedBy = assignment.AssignedBy,
            AssignedDate = assignment.AssignedDate,
            StartedDate = assignment.StartedDate,
            CompletedDate = assignment.CompletedDate,
            DueDate = assignment.DueDate,
            EmployeeEmail = assignment.EmployeeEmail,
            EmployeeId = assignment.EmployeeId.ToString(),
            EmployeeName = assignment.EmployeeName,
            Id = assignment.Id,
            Notes = assignment.Notes,
            TemplateId = assignment.TemplateId,
            ProcessType = MapProcessType(assignment.ProcessType),
            TemplateName = assignment.TemplateName,
            TemplateCategoryId = assignment.TemplateCategoryId,

            // Withdrawal tracking
            IsWithdrawn = assignment.IsWithdrawn,
            WithdrawnDate = assignment.WithdrawnDate,
            WithdrawnByEmployeeId = assignment.WithdrawnByEmployeeId,
            WithdrawnByEmployeeName = assignment.WithdrawnByEmployeeName,
            WithdrawalReason = assignment.WithdrawalReason,

            // Workflow properties
            WorkflowState = assignment.WorkflowState,
            SectionProgress = assignment.SectionProgress,

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

            // Reopen tracking (audit trail)
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
    /// Same pattern used by QuestionnaireTemplatesController for consistency.
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