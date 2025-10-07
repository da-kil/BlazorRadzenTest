using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class GetTeamAnalyticsQueryHandler : IQueryHandler<GetTeamAnalyticsQuery, Result<TeamAnalytics>>
{
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;
    private readonly IEmployeeRepository employeeRepository;
    private readonly ILogger<GetTeamAnalyticsQueryHandler> logger;

    public GetTeamAnalyticsQueryHandler(
        IQuestionnaireAssignmentRepository assignmentRepository,
        IEmployeeRepository employeeRepository,
        ILogger<GetTeamAnalyticsQueryHandler> logger)
    {
        this.assignmentRepository = assignmentRepository;
        this.employeeRepository = employeeRepository;
        this.logger = logger;
    }

    public async Task<Result<TeamAnalytics>> HandleAsync(GetTeamAnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Generating team analytics for manager {ManagerId}", query.ManagerId);

        try
        {
            // Get all team members for this manager
            var managerIdStr = query.ManagerId.ToString();
            var teamMembers = await employeeRepository.GetEmployeesByManagerIdAsync(managerIdStr, cancellationToken);
            var activeTeamMembers = teamMembers.Where(e => !e.IsDeleted).ToList();

            if (!activeTeamMembers.Any())
            {
                logger.LogInformation("No team members found for manager {ManagerId}", query.ManagerId);
                return Result<TeamAnalytics>.Success(new TeamAnalytics());
            }

            var teamMemberIds = activeTeamMembers.Select(e => e.Id).ToList();

            // Get all assignments for team members
            var allAssignmentReadModels = new List<Application.Query.Projections.QuestionnaireAssignmentReadModel>();
            foreach (var employeeId in teamMemberIds)
            {
                var employeeAssignments = await assignmentRepository.GetAssignmentsByEmployeeIdAsync(employeeId, cancellationToken);
                allAssignmentReadModels.AddRange(employeeAssignments);
            }

            var allAssignments = allAssignmentReadModels.ToList();

            var now = DateTime.Now;
            var completedAssignments = allAssignments.Where(a => a.Status == AssignmentStatus.Completed).ToList();
            var overdueAssignments = allAssignments.Where(a =>
                a.DueDate.HasValue &&
                a.DueDate.Value < now &&
                a.Status != AssignmentStatus.Completed &&
                !a.IsWithdrawn).ToList();

            // Calculate average completion time
            var completedWithTime = completedAssignments
                .Where(a => a.StartedDate.HasValue && a.CompletedDate.HasValue)
                .Select(a => (a.CompletedDate!.Value - a.StartedDate!.Value).TotalDays)
                .ToList();

            var averageCompletionTime = completedWithTime.Any() ? completedWithTime.Average() : 0;

            // Calculate on-time completion rate
            var onTimeCompleted = completedAssignments.Count(a =>
                !a.DueDate.HasValue ||
                (a.CompletedDate.HasValue && a.CompletedDate.Value <= a.DueDate.Value));

            var onTimeRate = completedAssignments.Any()
                ? (double)onTimeCompleted / completedAssignments.Count * 100
                : 0;

            // Category performance (simplified - group by template)
            var categoryPerformance = allAssignments
                .GroupBy(a => a.TemplateId)
                .Select(g =>
                {
                    var total = g.Count();
                    var completed = g.Count(a => a.Status == AssignmentStatus.Completed);
                    var completedWithTimes = g
                        .Where(a => a.StartedDate.HasValue && a.CompletedDate.HasValue)
                        .Select(a => (a.CompletedDate!.Value - a.StartedDate!.Value).TotalDays)
                        .ToList();

                    return new CategoryPerformance
                    {
                        Category = g.Key.ToString(), // TODO: Fetch template name if needed
                        TotalAssignments = total,
                        CompletedAssignments = completed,
                        CompletionRate = total > 0 ? (double)completed / total * 100 : 0,
                        AverageCompletionTime = completedWithTimes.Any() ? completedWithTimes.Average() : 0
                    };
                })
                .ToList();

            // Employee performance
            var employeePerformance = teamMemberIds.Select(employeeId =>
            {
                var employee = activeTeamMembers.First(e => e.Id == employeeId);
                var employeeAssignments = allAssignments.Where(a => a.EmployeeId == employeeId).ToList();
                var employeeCompleted = employeeAssignments.Count(a => a.Status == AssignmentStatus.Completed);
                var employeeOverdue = employeeAssignments.Count(a =>
                    a.DueDate.HasValue &&
                    a.DueDate.Value < now &&
                    a.Status != AssignmentStatus.Completed &&
                    !a.IsWithdrawn);

                var employeeCompletedWithTimes = employeeAssignments
                    .Where(a => a.StartedDate.HasValue && a.CompletedDate.HasValue)
                    .Select(a => (a.CompletedDate!.Value - a.StartedDate!.Value).TotalDays)
                    .ToList();

                return new EmployeePerformance
                {
                    EmployeeId = employeeId.ToString(),
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    TotalAssignments = employeeAssignments.Count,
                    CompletedAssignments = employeeCompleted,
                    OverdueAssignments = employeeOverdue,
                    CompletionRate = employeeAssignments.Any() ? (double)employeeCompleted / employeeAssignments.Count * 100 : 0,
                    AverageCompletionTime = employeeCompletedWithTimes.Any() ? employeeCompletedWithTimes.Average() : 0
                };
            }).ToList();

            var analytics = new TeamAnalytics
            {
                TotalTeamMembers = activeTeamMembers.Count,
                TotalAssignments = allAssignments.Count,
                CompletedAssignments = completedAssignments.Count,
                OverdueAssignments = overdueAssignments.Count,
                AverageCompletionTime = averageCompletionTime,
                OnTimeCompletionRate = onTimeRate,
                CategoryPerformance = categoryPerformance,
                EmployeePerformance = employeePerformance
            };

            logger.LogInformation("Generated team analytics for manager {ManagerId}: {TotalMembers} members, {TotalAssignments} assignments",
                query.ManagerId, analytics.TotalTeamMembers, analytics.TotalAssignments);

            return Result<TeamAnalytics>.Success(analytics);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate team analytics for manager {ManagerId}", query.ManagerId);
            return Result<TeamAnalytics>.Fail($"Failed to generate team analytics: {ex.Message}", 500);
        }
    }
}
