using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.HRQueries;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/hr")]
[Authorize(Roles = "HR,HRLead,Admin")]
public class HRController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<HRController> logger;

    public HRController(
        IQueryDispatcher queryDispatcher,
        ILogger<HRController> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    /// <summary>
    /// Gets the HR dashboard with organization-wide metrics and analytics.
    /// Available to HR and Admin roles only.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(HRDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHRDashboard()
    {
        logger.LogInformation("Received GetHRDashboard request");

        try
        {
            var result = await queryDispatcher.QueryAsync(new HRDashboardQuery());

            if (result?.Payload == null)
            {
                logger.LogInformation("HR dashboard not found - this is expected for new systems");

                // Return empty dashboard for systems with no data yet
                return Ok(new HRDashboardDto
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
            }
            else
            {
                logger.LogWarning("GetHRDashboard failed, Error: {ErrorMessage}", result.Message);
            }

            return CreateResponse(result, dashboard => new HRDashboardDto
            {
                TotalEmployees = dashboard.TotalEmployees,
                TotalManagers = dashboard.TotalManagers,
                TotalAssignments = dashboard.TotalAssignments,
                TotalPendingAssignments = dashboard.TotalPendingAssignments,
                TotalInProgressAssignments = dashboard.TotalInProgressAssignments,
                TotalCompletedAssignments = dashboard.TotalCompletedAssignments,
                TotalOverdueAssignments = dashboard.TotalOverdueAssignments,
                OverallCompletionRate = dashboard.OverallCompletionRate,
                AverageCompletionTimeInDays = dashboard.AverageCompletionTimeInDays,
                Organizations = dashboard.Organizations.Select(o => new OrganizationMetricsDto
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
                Managers = dashboard.Managers.Select(m => new ManagerOverviewDto
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
                AssignmentsCreatedLast7Days = dashboard.AssignmentsCreatedLast7Days,
                AssignmentsCompletedLast7Days = dashboard.AssignmentsCompletedLast7Days,
                UrgentAssignments = dashboard.UrgentAssignments.Select(ua => new UrgentAssignmentDto
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
                LastUpdated = dashboard.LastUpdated
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving HR dashboard");
            return StatusCode(500, "An error occurred while retrieving the HR dashboard");
        }
    }
}
