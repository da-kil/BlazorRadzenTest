using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.HRQueries;

public class HRDashboardQueryHandler : IQueryHandler<HRDashboardQuery, Result<HRDashboard?>>
{
    private readonly IHRDashboardRepository repository;
    private readonly ILogger<HRDashboardQueryHandler> logger;

    public HRDashboardQueryHandler(
        IHRDashboardRepository repository,
        ILogger<HRDashboardQueryHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result<HRDashboard?>> HandleAsync(HRDashboardQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogHRDashboardQueryStarting();

        try
        {
            var dashboardReadModel = await repository.GetHRDashboardAsync(cancellationToken);

            if (dashboardReadModel != null)
            {
                var dashboard = new HRDashboard
                {
                    TotalEmployees = dashboardReadModel.TotalEmployees,
                    TotalManagers = dashboardReadModel.TotalManagers,
                    TotalAssignments = dashboardReadModel.TotalAssignments,
                    TotalPendingAssignments = dashboardReadModel.TotalPendingAssignments,
                    TotalInProgressAssignments = dashboardReadModel.TotalInProgressAssignments,
                    TotalCompletedAssignments = dashboardReadModel.TotalCompletedAssignments,
                    TotalOverdueAssignments = dashboardReadModel.TotalOverdueAssignments,
                    OverallCompletionRate = dashboardReadModel.OverallCompletionRate,
                    AverageCompletionTimeInDays = dashboardReadModel.AverageCompletionTimeInDays,
                    Organizations = dashboardReadModel.Organizations.Select(o => new HRDashboard.OrganizationMetrics
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
                    Managers = dashboardReadModel.Managers.Select(m => new HRDashboard.ManagerOverview
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
                    AssignmentsCreatedLast7Days = dashboardReadModel.AssignmentsCreatedLast7Days,
                    AssignmentsCompletedLast7Days = dashboardReadModel.AssignmentsCompletedLast7Days,
                    UrgentAssignments = dashboardReadModel.UrgentAssignments.Select(ua => new HRDashboard.UrgentAssignment
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
                    LastUpdated = dashboardReadModel.LastUpdated
                };

                logger.LogHRDashboardQuerySucceeded();
                return Result<HRDashboard?>.Success(dashboard);
            }

            logger.LogHRDashboardNotFound();
            return Result<HRDashboard?>.Success(null);
        }
        catch (Exception ex)
        {
            logger.LogHRDashboardQueryFailed(ex);
            return Result<HRDashboard?>.Fail($"Failed to retrieve HR dashboard: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}
