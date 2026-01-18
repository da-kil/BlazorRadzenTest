using Microsoft.AspNetCore.Authorization;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;
using ti8m.BeachBreak.QueryApi.Authorization;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for manager queries.
/// </summary>
public static class ManagersEndpoints
{
    /// <summary>
    /// Maps manager query endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapManagersEndpoints(this WebApplication app)
    {
        var managersGroup = app.MapGroup("/q/api/v{version:apiVersion}/managers")
            .WithTags("Managers")
            .RequireAuthorization("TeamLead"); // Base authorization for all manager endpoints

        // Get my dashboard - authenticated manager only
        managersGroup.MapGet("/me/dashboard", async (
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            Guid managerId;
            try
            {
                managerId = await authorizationService.GetCurrentManagerIdAsync();
                logger.LogInformation("Received GetMyDashboard request for authenticated ManagerId: {ManagerId}", managerId);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetMyDashboard failed: {Message}", ex.Message);
                return Results.Unauthorized();
            }

            try
            {
                var result = await queryDispatcher.QueryAsync(new ManagerDashboardQuery(managerId), cancellationToken);

                if (result?.Payload == null)
                {
                    logger.LogInformation("Dashboard not found for ManagerId: {ManagerId} - this is expected for new managers or managers with no team", managerId);

                    // Return empty dashboard for managers with no team yet
                    return Results.Ok(new ManagerDashboardDto
                    {
                        ManagerId = managerId,
                        ManagerFullName = string.Empty,
                        ManagerEmail = string.Empty,
                        TeamMemberCount = 0,
                        TeamPendingCount = 0,
                        TeamInProgressCount = 0,
                        TeamCompletedCount = 0,
                        TeamMembers = new List<TeamMemberMetricsDto>(),
                        UrgentAssignments = new List<TeamUrgentAssignmentDto>(),
                        LastUpdated = DateTime.UtcNow
                    });
                }

                if (result.Succeeded)
                {
                    logger.LogInformation("GetMyDashboard completed successfully for ManagerId: {ManagerId}", managerId);

                    var dashboard = new ManagerDashboardDto
                    {
                        ManagerId = result.Payload.ManagerId,
                        ManagerFullName = result.Payload.ManagerFullName,
                        ManagerEmail = result.Payload.ManagerEmail,
                        TeamPendingCount = result.Payload.TeamPendingCount,
                        TeamInProgressCount = result.Payload.TeamInProgressCount,
                        TeamCompletedCount = result.Payload.TeamCompletedCount,
                        TeamMemberCount = result.Payload.TeamMemberCount,
                        TeamMembers = result.Payload.TeamMembers.Select(tm => new TeamMemberMetricsDto
                        {
                            EmployeeId = tm.EmployeeId,
                            EmployeeName = tm.EmployeeName,
                            EmployeeEmail = tm.EmployeeEmail,
                            PendingCount = tm.PendingCount,
                            InProgressCount = tm.InProgressCount,
                            CompletedCount = tm.CompletedCount,
                            UrgentCount = tm.UrgentCount,
                            HasOverdueItems = tm.HasOverdueItems
                        }).ToList(),
                        UrgentAssignments = result.Payload.UrgentAssignments.Select(ua => new TeamUrgentAssignmentDto
                        {
                            AssignmentId = ua.AssignmentId,
                            EmployeeId = ua.EmployeeId,
                            EmployeeName = ua.EmployeeName,
                            QuestionnaireTemplateName = ua.QuestionnaireTemplateName,
                            DueDate = ua.DueDate,
                            WorkflowState = ua.WorkflowState,
                            IsOverdue = ua.IsOverdue,
                            DaysUntilDue = ua.DaysUntilDue
                        }).ToList(),
                        LastUpdated = result.Payload.LastUpdated
                    };

                    return Results.Ok(dashboard);
                }
                else
                {
                    logger.LogWarning("GetMyDashboard failed for ManagerId: {ManagerId}, Error: {ErrorMessage}", managerId, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving dashboard for manager {ManagerId}", managerId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving your dashboard",
                    statusCode: 500);
            }
        })
        .WithName("GetMyDashboard")
        .WithSummary("Get my dashboard")
        .WithDescription("Gets the dashboard metrics for the authenticated manager - includes team-wide metrics, individual team member metrics, and urgent assignments")
        .Produces<ManagerDashboardDto>(200)
        .Produces(401)
        .Produces(404)
        .Produces(500);

        // Get my team members - authenticated manager only
        managersGroup.MapGet("/me/team", async (
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            Guid managerId;
            try
            {
                managerId = await authorizationService.GetCurrentManagerIdAsync();
                logger.LogInformation("Received GetMyTeamMembers request for authenticated ManagerId: {ManagerId}", managerId);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetMyTeamMembers failed: {Message}", ex.Message);
                return Results.Unauthorized();
            }

            try
            {
                var query = new GetTeamMembersQuery(managerId);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result.Succeeded)
                {
                    var teamCount = result.Payload?.Count() ?? 0;
                    logger.LogInformation("GetTeamMembers completed successfully for ManagerId: {ManagerId}, returned {TeamCount} members",
                        managerId, teamCount);

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
                else
                {
                    logger.LogWarning("GetTeamMembers failed for ManagerId: {ManagerId}, Error: {ErrorMessage}",
                        managerId, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving team members for manager {ManagerId}", managerId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving team members",
                    statusCode: 500);
            }
        })
        .WithName("GetMyTeamMembers")
        .WithSummary("Get my team members")
        .WithDescription("Gets all team members (direct reports) for the authenticated manager")
        .Produces<IEnumerable<EmployeeDto>>(200)
        .Produces(401)
        .Produces(403)
        .Produces(500);

        // Get my team assignments - authenticated manager only
        managersGroup.MapGet("/me/assignments", async (
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            ILogger logger,
            string? workflowState = null,
            CancellationToken cancellationToken = default) =>
        {
            Guid managerId;
            try
            {
                managerId = await authorizationService.GetCurrentManagerIdAsync();
                logger.LogInformation("Received GetMyTeamAssignments request for authenticated ManagerId: {ManagerId}, WorkflowState: {WorkflowState}",
                    managerId, workflowState);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetMyTeamAssignments failed: {Message}", ex.Message);
                return Results.Unauthorized();
            }

            try
            {
                WorkflowState? filterWorkflowState = null;
                if (!string.IsNullOrWhiteSpace(workflowState) && Enum.TryParse<WorkflowState>(workflowState, true, out var parsedState))
                {
                    filterWorkflowState = parsedState;
                }

                var query = new GetTeamAssignmentsQuery(managerId, filterWorkflowState);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result.Succeeded)
                {
                    var assignmentCount = result.Payload?.Count() ?? 0;
                    logger.LogInformation("GetTeamAssignments completed successfully for ManagerId: {ManagerId}, returned {AssignmentCount} assignments",
                        managerId, assignmentCount);

                    var assignments = result.Payload.Select(assignment => new TeamAssignmentDto
                    {
                        Id = assignment.Id,
                        EmployeeId = assignment.EmployeeId.ToString(),
                        EmployeeName = assignment.EmployeeName,
                        EmployeeEmail = assignment.EmployeeEmail,
                        TemplateId = assignment.TemplateId,
                        TemplateName = assignment.TemplateName,
                        TemplateCategoryId = assignment.TemplateCategoryId,
                        AssignedDate = assignment.AssignedDate,
                        DueDate = assignment.DueDate,
                        CompletedDate = assignment.CompletedDate,
                        AssignedBy = assignment.AssignedBy,
                        Notes = assignment.Notes,

                        // Workflow properties
                        WorkflowState = assignment.WorkflowState,
                        SectionProgress = assignment.SectionProgress,

                        // Submission phase
                        EmployeeSubmittedDate = assignment.EmployeeSubmittedDate,
                        EmployeeSubmittedBy = assignment.EmployeeSubmittedByEmployeeName,
                        ManagerSubmittedDate = assignment.ManagerSubmittedDate,
                        ManagerSubmittedBy = assignment.ManagerSubmittedByEmployeeName,

                        // Review phase
                        ReviewInitiatedDate = assignment.ReviewInitiatedDate,
                        ReviewInitiatedBy = assignment.ReviewInitiatedByEmployeeName,
                        ManagerReviewFinishedDate = assignment.ManagerReviewFinishedDate,
                        ManagerReviewFinishedBy = assignment.ManagerReviewFinishedByEmployeeName,
                        ManagerReviewSummary = assignment.ManagerReviewSummary,
                        EmployeeReviewConfirmedDate = assignment.EmployeeReviewConfirmedDate,
                        EmployeeReviewConfirmedBy = assignment.EmployeeReviewConfirmedByEmployeeName,
                        EmployeeReviewComments = assignment.EmployeeReviewComments,

                        // Final state
                        FinalizedDate = assignment.FinalizedDate,
                        FinalizedBy = assignment.FinalizedByEmployeeName,
                        ManagerFinalNotes = assignment.ManagerFinalNotes,
                        IsLocked = assignment.IsLocked
                    });

                    return Results.Ok(assignments);
                }
                else
                {
                    logger.LogWarning("GetTeamAssignments failed for ManagerId: {ManagerId}, Error: {ErrorMessage}",
                        managerId, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving team assignments for manager {ManagerId}", managerId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving team assignments",
                    statusCode: 500);
            }
        })
        .WithName("GetMyTeamAssignments")
        .WithSummary("Get my team assignments")
        .WithDescription("Gets all questionnaire assignments for the authenticated manager's team - supports workflow state filtering")
        .Produces<IEnumerable<TeamAssignmentDto>>(200)
        .Produces(401)
        .Produces(403)
        .Produces(500);

        // Get my team progress - authenticated manager only
        managersGroup.MapGet("/me/team/progress", async (
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            Guid managerId;
            try
            {
                managerId = await authorizationService.GetCurrentManagerIdAsync();
                logger.LogInformation("Received GetMyTeamProgress request for authenticated ManagerId: {ManagerId}", managerId);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetMyTeamProgress failed: {Message}", ex.Message);
                return Results.Unauthorized();
            }

            try
            {
                var query = new GetTeamProgressQuery(managerId);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result.Succeeded)
                {
                    var progressCount = result.Payload?.Count() ?? 0;
                    logger.LogInformation("GetTeamProgress completed successfully for ManagerId: {ManagerId}, returned {ProgressCount} items",
                        managerId, progressCount);

                    var progressItems = result.Payload.Select(progress => new AssignmentProgressDto
                    {
                        AssignmentId = progress.AssignmentId,
                        ProgressPercentage = progress.ProgressPercentage,
                        TotalQuestions = progress.TotalQuestions,
                        AnsweredQuestions = progress.AnsweredQuestions,
                        LastModified = progress.LastModified,
                        IsCompleted = progress.IsCompleted,
                        TimeSpent = progress.TimeSpent
                    });

                    return Results.Ok(progressItems);
                }
                else
                {
                    logger.LogWarning("GetTeamProgress failed for ManagerId: {ManagerId}, Error: {ErrorMessage}",
                        managerId, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving team progress for manager {ManagerId}", managerId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving team progress",
                    statusCode: 500);
            }
        })
        .WithName("GetMyTeamProgress")
        .WithSummary("Get my team progress")
        .WithDescription("Gets progress data for all assignments in the authenticated manager's team")
        .Produces<IEnumerable<AssignmentProgressDto>>(200)
        .Produces(401)
        .Produces(403)
        .Produces(500);

        // Get my team analytics - authenticated manager only
        managersGroup.MapGet("/me/analytics", async (
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            Guid managerId;
            try
            {
                managerId = await authorizationService.GetCurrentManagerIdAsync();
                logger.LogInformation("Received GetMyTeamAnalytics request for authenticated ManagerId: {ManagerId}", managerId);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetMyTeamAnalytics failed: {Message}", ex.Message);
                return Results.Unauthorized();
            }

            try
            {
                var query = new GetTeamAnalyticsQuery(managerId);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result.Succeeded)
                {
                    logger.LogInformation("GetTeamAnalytics completed successfully for ManagerId: {ManagerId}", managerId);

                    var analytics = new TeamAnalyticsDto
                    {
                        TotalTeamMembers = result.Payload.TotalTeamMembers,
                        TotalAssignments = result.Payload.TotalAssignments,
                        CompletedAssignments = result.Payload.CompletedAssignments,
                        OverdueAssignments = result.Payload.OverdueAssignments,
                        AverageCompletionTime = result.Payload.AverageCompletionTime,
                        OnTimeCompletionRate = result.Payload.OnTimeCompletionRate,
                        CategoryPerformance = result.Payload.CategoryPerformance.Select(cp => new CategoryPerformanceDto
                        {
                            Category = cp.Category,
                            TotalAssignments = cp.TotalAssignments,
                            CompletedAssignments = cp.CompletedAssignments,
                            CompletionRate = cp.CompletionRate,
                            AverageCompletionTime = cp.AverageCompletionTime
                        }).ToList(),
                        EmployeePerformance = result.Payload.EmployeePerformance.Select(ep => new EmployeePerformanceDto
                        {
                            EmployeeId = ep.EmployeeId,
                            EmployeeName = ep.EmployeeName,
                            TotalAssignments = ep.TotalAssignments,
                            CompletedAssignments = ep.CompletedAssignments,
                            OverdueAssignments = ep.OverdueAssignments,
                            CompletionRate = ep.CompletionRate,
                            AverageCompletionTime = ep.AverageCompletionTime
                        }).ToList()
                    };

                    return Results.Ok(analytics);
                }
                else
                {
                    logger.LogWarning("GetTeamAnalytics failed for ManagerId: {ManagerId}, Error: {ErrorMessage}",
                        managerId, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving team analytics for manager {ManagerId}", managerId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving team analytics",
                    statusCode: 500);
            }
        })
        .WithName("GetMyTeamAnalytics")
        .WithSummary("Get my team analytics")
        .WithDescription("Gets analytics data for the authenticated manager's team")
        .Produces<TeamAnalyticsDto>(200)
        .Produces(401)
        .Produces(403)
        .Produces(500);

        // Get specific manager's team - HR only
        managersGroup.MapGet("/{managerId:guid}/team", async (
            Guid managerId,
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            Guid requestingUserId;
            try
            {
                requestingUserId = await authorizationService.GetCurrentManagerIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetManagerTeamMembers failed: {Message}", ex.Message);
                return Results.Unauthorized();
            }

            // Check authorization
            if (!await authorizationService.CanViewTeamAsync(requestingUserId, managerId))
            {
                logger.LogWarning("User {RequestingUserId} not authorized to view manager {ManagerId} team",
                    requestingUserId, managerId);
                return Results.Forbid();
            }

            logger.LogInformation("User {RequestingUserId} viewing team for ManagerId: {ManagerId}",
                requestingUserId, managerId);

            try
            {
                var query = new GetTeamMembersQuery(managerId);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result.Succeeded)
                {
                    var teamCount = result.Payload?.Count() ?? 0;
                    logger.LogInformation("GetTeamMembers completed successfully for ManagerId: {ManagerId}, returned {TeamCount} members",
                        managerId, teamCount);

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
                else
                {
                    logger.LogWarning("GetTeamMembers failed for ManagerId: {ManagerId}, Error: {ErrorMessage}",
                        managerId, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving team members for manager {ManagerId}", managerId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving team members",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("HR") // Override to HR for this specific endpoint
        .WithName("GetManagerTeamMembers")
        .WithSummary("Get manager's team members")
        .WithDescription("Gets all team members for a specific manager - HR/Admin only")
        .Produces<IEnumerable<EmployeeDto>>(200)
        .Produces(401)
        .Produces(403)
        .Produces(500);

        // Get specific manager's team assignments - HR only
        managersGroup.MapGet("/{managerId:guid}/assignments", async (
            Guid managerId,
            IQueryDispatcher queryDispatcher,
            IManagerAuthorizationService authorizationService,
            ILogger logger,
            string? workflowState = null,
            CancellationToken cancellationToken = default) =>
        {
            Guid requestingUserId;
            try
            {
                requestingUserId = await authorizationService.GetCurrentManagerIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("GetManagerTeamAssignments failed: {Message}", ex.Message);
                return Results.Unauthorized();
            }

            // Check authorization
            if (!await authorizationService.CanViewTeamAsync(requestingUserId, managerId))
            {
                logger.LogWarning("User {RequestingUserId} not authorized to view manager {ManagerId} team assignments",
                    requestingUserId, managerId);
                return Results.Forbid();
            }

            logger.LogInformation("User {RequestingUserId} viewing team assignments for ManagerId: {ManagerId}, WorkflowState: {WorkflowState}",
                requestingUserId, managerId, workflowState);

            try
            {
                WorkflowState? filterWorkflowState = null;
                if (!string.IsNullOrWhiteSpace(workflowState) && Enum.TryParse<WorkflowState>(workflowState, true, out var parsedState))
                {
                    filterWorkflowState = parsedState;
                }

                var query = new GetTeamAssignmentsQuery(managerId, filterWorkflowState);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result.Succeeded)
                {
                    var assignmentCount = result.Payload?.Count() ?? 0;
                    logger.LogInformation("GetTeamAssignments completed successfully for ManagerId: {ManagerId}, returned {AssignmentCount} assignments",
                        managerId, assignmentCount);

                    var assignments = result.Payload.Select(assignment => new TeamAssignmentDto
                    {
                        Id = assignment.Id,
                        EmployeeId = assignment.EmployeeId.ToString(),
                        EmployeeName = assignment.EmployeeName,
                        EmployeeEmail = assignment.EmployeeEmail,
                        TemplateId = assignment.TemplateId,
                        TemplateName = assignment.TemplateName,
                        TemplateCategoryId = assignment.TemplateCategoryId,
                        AssignedDate = assignment.AssignedDate,
                        DueDate = assignment.DueDate,
                        CompletedDate = assignment.CompletedDate,
                        AssignedBy = assignment.AssignedBy,
                        Notes = assignment.Notes,

                        // Workflow properties
                        WorkflowState = assignment.WorkflowState,
                        SectionProgress = assignment.SectionProgress,

                        // Submission phase
                        EmployeeSubmittedDate = assignment.EmployeeSubmittedDate,
                        EmployeeSubmittedBy = assignment.EmployeeSubmittedByEmployeeName,
                        ManagerSubmittedDate = assignment.ManagerSubmittedDate,
                        ManagerSubmittedBy = assignment.ManagerSubmittedByEmployeeName,

                        // Review phase
                        ReviewInitiatedDate = assignment.ReviewInitiatedDate,
                        ReviewInitiatedBy = assignment.ReviewInitiatedByEmployeeName,
                        ManagerReviewFinishedDate = assignment.ManagerReviewFinishedDate,
                        ManagerReviewFinishedBy = assignment.ManagerReviewFinishedByEmployeeName,
                        ManagerReviewSummary = assignment.ManagerReviewSummary,
                        EmployeeReviewConfirmedDate = assignment.EmployeeReviewConfirmedDate,
                        EmployeeReviewConfirmedBy = assignment.EmployeeReviewConfirmedByEmployeeName,
                        EmployeeReviewComments = assignment.EmployeeReviewComments,

                        // Final state
                        FinalizedDate = assignment.FinalizedDate,
                        FinalizedBy = assignment.FinalizedByEmployeeName,
                        ManagerFinalNotes = assignment.ManagerFinalNotes,
                        IsLocked = assignment.IsLocked
                    });

                    return Results.Ok(assignments);
                }
                else
                {
                    logger.LogWarning("GetTeamAssignments failed for ManagerId: {ManagerId}, Error: {ErrorMessage}",
                        managerId, result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving team assignments for manager {ManagerId}", managerId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving team assignments",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("HR") // Override to HR for this specific endpoint
        .WithName("GetManagerTeamAssignments")
        .WithSummary("Get manager's team assignments")
        .WithDescription("Gets all questionnaire assignments for a specific manager's team - HR/Admin only, supports workflow state filtering")
        .Produces<IEnumerable<TeamAssignmentDto>>(200)
        .Produces(401)
        .Produces(403)
        .Produces(500);
    }
}