using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeDashboardQueryHandler : IQueryHandler<EmployeeDashboardQuery, Result<EmployeeDashboard?>>
{
    private readonly IEmployeeDashboardRepository repository;
    private readonly ILogger<EmployeeDashboardQueryHandler> logger;

    public EmployeeDashboardQueryHandler(
        IEmployeeDashboardRepository repository,
        ILogger<EmployeeDashboardQueryHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result<EmployeeDashboard?>> HandleAsync(EmployeeDashboardQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogEmployeeDashboardQueryStarting(query.EmployeeId);

        try
        {
            var dashboardReadModel = await repository.GetDashboardByEmployeeIdAsync(query.EmployeeId, cancellationToken);

            if (dashboardReadModel != null)
            {
                var dashboard = new EmployeeDashboard
                {
                    EmployeeId = dashboardReadModel.Id,
                    EmployeeFullName = dashboardReadModel.EmployeeFullName,
                    EmployeeEmail = dashboardReadModel.EmployeeEmail,
                    PendingCount = dashboardReadModel.PendingCount,
                    InProgressCount = dashboardReadModel.InProgressCount,
                    CompletedCount = dashboardReadModel.CompletedCount,
                    UrgentAssignments = dashboardReadModel.UrgentAssignments.Select(ua => new EmployeeDashboard.UrgentAssignment
                    {
                        AssignmentId = ua.AssignmentId,
                        QuestionnaireTemplateName = ua.QuestionnaireTemplateName,
                        DueDate = ua.DueDate,
                        WorkflowState = ua.WorkflowState.ToString(),
                        IsOverdue = ua.IsOverdue
                    }).ToList(),
                    LastUpdated = dashboardReadModel.LastUpdated
                };

                logger.LogEmployeeDashboardQuerySucceeded(query.EmployeeId);
                return Result<EmployeeDashboard?>.Success(dashboard);
            }

            logger.LogEmployeeDashboardNotFound(query.EmployeeId);
            return Result<EmployeeDashboard?>.Success(null);
        }
        catch (Exception ex)
        {
            logger.LogEmployeeDashboardQueryFailed(query.EmployeeId, ex);
            return Result<EmployeeDashboard?>.Fail($"Failed to retrieve employee dashboard: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}
