using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class ManagerDashboardQueryHandler : IQueryHandler<ManagerDashboardQuery, Result<ManagerDashboard?>>
{
    private readonly IManagerDashboardRepository repository;
    private readonly ILogger<ManagerDashboardQueryHandler> logger;

    public ManagerDashboardQueryHandler(
        IManagerDashboardRepository repository,
        ILogger<ManagerDashboardQueryHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result<ManagerDashboard?>> HandleAsync(ManagerDashboardQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogManagerDashboardQueryStarting(query.ManagerId);

        try
        {
            var dashboardReadModel = await repository.GetDashboardByManagerIdAsync(query.ManagerId, cancellationToken);

            if (dashboardReadModel != null)
            {
                var dashboard = new ManagerDashboard
                {
                    ManagerId = dashboardReadModel.ManagerId,
                    ManagerFullName = dashboardReadModel.ManagerFullName,
                    ManagerEmail = dashboardReadModel.ManagerEmail,
                    TeamPendingCount = dashboardReadModel.TeamPendingCount,
                    TeamInProgressCount = dashboardReadModel.TeamInProgressCount,
                    TeamCompletedCount = dashboardReadModel.TeamCompletedCount,
                    TeamMemberCount = dashboardReadModel.TeamMemberCount,
                    TeamMembers = dashboardReadModel.TeamMembers.Select(tm => new ManagerDashboard.TeamMemberMetrics
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
                    UrgentAssignments = dashboardReadModel.UrgentAssignments.Select(ua => new ManagerDashboard.TeamUrgentAssignment
                    {
                        AssignmentId = ua.AssignmentId,
                        EmployeeId = ua.EmployeeId,
                        EmployeeName = ua.EmployeeName,
                        QuestionnaireTemplateName = ua.QuestionnaireTemplateName,
                        DueDate = ua.DueDate,
                        WorkflowState = ua.WorkflowState.ToString(),
                        IsOverdue = ua.IsOverdue,
                        DaysUntilDue = ua.DaysUntilDue
                    }).ToList(),
                    LastUpdated = dashboardReadModel.LastUpdated
                };

                logger.LogManagerDashboardQuerySucceeded(query.ManagerId);
                return Result<ManagerDashboard?>.Success(dashboard);
            }

            logger.LogManagerDashboardNotFound(query.ManagerId);
            return Result<ManagerDashboard?>.Success(null);
        }
        catch (Exception ex)
        {
            logger.LogManagerDashboardQueryFailed(query.ManagerId, ex);
            return Result<ManagerDashboard?>.Fail($"Failed to retrieve manager dashboard: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}
