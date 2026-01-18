using Microsoft.AspNetCore.Authorization;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.HRQueries;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for HR queries.
/// </summary>
public static class HREndpoints
{
    /// <summary>
    /// Maps HR query endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapHREndpoints(this WebApplication app)
    {
        var hrGroup = app.MapGroup("/q/api/v{version:apiVersion}/hr")
            .WithTags("HR")
            .RequireAuthorization("HR");

        // Get HR dashboard
        hrGroup.MapGet("/dashboard", async (
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            logger.LogInformation("Received GetHRDashboard request");

            try
            {
                var result = await queryDispatcher.QueryAsync(new HRDashboardQuery(), cancellationToken);

                if (result?.Payload == null)
                {
                    logger.LogInformation("HR dashboard not found - this is expected for new systems");

                    // Return empty dashboard for systems with no data yet
                    return Results.Ok(new HRDashboardDto
                    {
                        TotalEmployees = 0,
                        TotalManagers = 0,
                        TotalAssignments = 0,
                        TotalPendingAssignments = 0,
                        TotalInProgressAssignments = 0,
                        TotalCompletedAssignments = 0,
                        TotalOverdueAssignments = 0,
                        OverallCompletionRate = 0.0,
                        AverageCompletionTimeInDays = 0.0,
                        Organizations = new List<OrganizationMetricsDto>(),
                        Managers = new List<ManagerOverviewDto>(),
                        AssignmentsCreatedLast7Days = 0,
                        AssignmentsCompletedLast7Days = 0,
                        UrgentAssignments = new List<UrgentAssignmentDto>(),
                        LastUpdated = DateTime.UtcNow
                    });
                }

                if (result.Succeeded)
                {
                    logger.LogInformation("GetHRDashboard completed successfully");

                    var dashboardDto = new HRDashboardDto
                    {
                        TotalEmployees = result.Payload.TotalEmployees,
                        TotalManagers = result.Payload.TotalManagers,
                        TotalAssignments = result.Payload.TotalAssignments,
                        TotalPendingAssignments = result.Payload.TotalPendingAssignments,
                        TotalInProgressAssignments = result.Payload.TotalInProgressAssignments,
                        TotalCompletedAssignments = result.Payload.TotalCompletedAssignments,
                        TotalOverdueAssignments = result.Payload.TotalOverdueAssignments,
                        OverallCompletionRate = result.Payload.OverallCompletionRate,
                        AverageCompletionTimeInDays = result.Payload.AverageCompletionTimeInDays,
                        Organizations = result.Payload.Organizations.Select(o => new OrganizationMetricsDto
                        {
                            OrganizationNumber = o.OrganizationNumber,
                            OrganizationName = o.OrganizationName,
                            EmployeeCount = o.EmployeeCount,
                            TotalAssignments = o.TotalAssignments,
                            PendingCount = o.PendingCount,
                            InProgressCount = o.InProgressCount,
                            CompletedCount = o.CompletedCount,
                            OverdueCount = o.OverdueCount,
                            CompletionRate = o.CompletionRate
                        }).ToList(),
                        Managers = result.Payload.Managers.Select(m => new ManagerOverviewDto
                        {
                            ManagerId = m.ManagerId,
                            ManagerName = m.ManagerName,
                            ManagerEmail = m.ManagerEmail,
                            TeamSize = m.TeamSize,
                            TotalAssignments = m.TotalAssignments,
                            CompletedAssignments = m.CompletedAssignments,
                            OverdueAssignments = m.OverdueAssignments,
                            CompletionRate = m.CompletionRate
                        }).ToList(),
                        AssignmentsCreatedLast7Days = result.Payload.AssignmentsCreatedLast7Days,
                        AssignmentsCompletedLast7Days = result.Payload.AssignmentsCompletedLast7Days,
                        UrgentAssignments = result.Payload.UrgentAssignments.Select(ua => new UrgentAssignmentDto
                        {
                            AssignmentId = ua.AssignmentId,
                            EmployeeId = ua.EmployeeId,
                            EmployeeName = ua.EmployeeName,
                            ManagerName = ua.ManagerName,
                            QuestionnaireTemplateName = ua.QuestionnaireTemplateName,
                            DueDate = ua.DueDate,
                            WorkflowState = ua.WorkflowState,
                            IsOverdue = ua.IsOverdue,
                            DaysUntilDue = ua.DaysUntilDue,
                            OrganizationName = ua.OrganizationName
                        }).ToList(),
                        LastUpdated = result.Payload.LastUpdated
                    };

                    return Results.Ok(dashboardDto);
                }
                else
                {
                    logger.LogWarning("GetHRDashboard failed, Error: {ErrorMessage}", result.Message);
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving HR dashboard");
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving the HR dashboard",
                    statusCode: 500);
            }
        })
        .WithName("GetHRDashboard")
        .WithSummary("Get HR dashboard")
        .WithDescription("Gets the HR dashboard with organization-wide metrics and analytics - HR and Admin roles only")
        .Produces<HRDashboardDto>(200)
        .Produces(401)
        .Produces(403)
        .Produces(500);
    }
}