using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

public class HRDashboardRepository : IHRDashboardRepository
{
    private readonly IDocumentSession session;

    public HRDashboardRepository(IDocumentSession session)
    {
        this.session = session;
    }

    public async Task<HRDashboardReadModel?> GetHRDashboardAsync(CancellationToken cancellationToken = default)
    {
        // Query all employees and assignments
        var allEmployees = await session.Query<EmployeeReadModel>()
            .Where(e => !e.IsDeleted)
            .ToListAsync(cancellationToken);

        var allAssignments = await session.Query<QuestionnaireAssignmentReadModel>()
            .ToListAsync(cancellationToken);

        var allOrganizations = await session.Query<OrganizationReadModel>()
            .Where(o => !o.IsDeleted && !o.IsIgnored)
            .ToListAsync(cancellationToken);

        // Get template names for lookups
        var templateIds = allAssignments.Select(a => a.TemplateId).Distinct().ToList();
        var templates = await session.Query<QuestionnaireTemplateReadModel>()
            .Where(t => templateIds.Contains(t.Id))
            .ToListAsync(cancellationToken);
        var templateLookup = templates.ToDictionary(t => t.Id, t => t.Name);

        // Calculate organization-wide metrics
        var totalEmployees = allEmployees.Count;
        var managers = allEmployees.Where(e => e.ApplicationRole == Domain.EmployeeAggregate.ApplicationRole.TeamLead).ToList();
        var totalManagers = managers.Count;

        var totalAssignments = allAssignments.Count;
        var pendingAssignments = allAssignments.Count(a => a.WorkflowState == WorkflowState.Assigned);
        var inProgressAssignments = allAssignments.Count(a =>
            a.WorkflowState == WorkflowState.EmployeeInProgress ||
            a.WorkflowState == WorkflowState.ManagerInProgress ||
            a.WorkflowState == WorkflowState.BothInProgress);
        var completedAssignments = allAssignments.Count(a => a.WorkflowState == WorkflowState.Finalized);

        var now = DateTime.UtcNow;
        var overdueAssignments = allAssignments.Count(a =>
            a.DueDate.HasValue &&
            a.DueDate.Value < now &&
            a.WorkflowState != WorkflowState.Finalized);

        // Completion rate
        var completionRate = totalAssignments > 0
            ? (double)completedAssignments / totalAssignments * 100
            : 0.0;

        // Average completion time
        var completedWithDates = allAssignments
            .Where(a => a.CompletedDate.HasValue && a.AssignedDate != default)
            .ToList();
        var averageCompletionTime = completedWithDates.Any()
            ? completedWithDates.Average(a => (a.CompletedDate!.Value - a.AssignedDate).TotalDays)
            : 0.0;

        // Organization breakdown
        var organizationMetrics = new List<OrganizationMetrics>();
        foreach (var org in allOrganizations)
        {
            var orgNumber = int.Parse(org.Number);
            var orgEmployees = allEmployees.Where(e => e.OrganizationNumber == orgNumber).ToList();
            var orgEmployeeIds = orgEmployees.Select(e => e.Id).ToHashSet();
            var orgAssignments = allAssignments.Where(a => orgEmployeeIds.Contains(a.EmployeeId)).ToList();

            var orgPending = orgAssignments.Count(a => a.WorkflowState == WorkflowState.Assigned);
            var orgInProgress = orgAssignments.Count(a =>
                a.WorkflowState == WorkflowState.EmployeeInProgress ||
                a.WorkflowState == WorkflowState.ManagerInProgress ||
                a.WorkflowState == WorkflowState.BothInProgress);
            var orgCompleted = orgAssignments.Count(a => a.WorkflowState == WorkflowState.Finalized);
            var orgOverdue = orgAssignments.Count(a =>
                a.DueDate.HasValue &&
                a.DueDate.Value < now &&
                a.WorkflowState != WorkflowState.Finalized);

            var orgCompletionRate = orgAssignments.Count > 0
                ? (double)orgCompleted / orgAssignments.Count * 100
                : 0.0;

            organizationMetrics.Add(new OrganizationMetrics
            {
                OrganizationNumber = orgNumber,
                OrganizationName = org.Name,
                EmployeeCount = orgEmployees.Count,
                TotalAssignments = orgAssignments.Count,
                PendingCount = orgPending,
                InProgressCount = orgInProgress,
                CompletedCount = orgCompleted,
                OverdueCount = orgOverdue,
                CompletionRate = Math.Round(orgCompletionRate, 1)
            });
        }

        // Manager overview
        var managerOverviews = new List<ManagerOverview>();
        foreach (var manager in managers)
        {
            var teamMembers = allEmployees.Where(e => e.ManagerId == manager.EmployeeId).ToList();
            var teamMemberIds = teamMembers.Select(e => e.Id).ToHashSet();
            var teamAssignments = allAssignments.Where(a => teamMemberIds.Contains(a.EmployeeId)).ToList();

            var teamCompleted = teamAssignments.Count(a => a.WorkflowState == WorkflowState.Finalized);
            var teamOverdue = teamAssignments.Count(a =>
                a.DueDate.HasValue &&
                a.DueDate.Value < now &&
                a.WorkflowState != WorkflowState.Finalized);

            var teamCompletionRate = teamAssignments.Count > 0
                ? (double)teamCompleted / teamAssignments.Count * 100
                : 0.0;

            managerOverviews.Add(new ManagerOverview
            {
                ManagerId = manager.Id,
                ManagerName = $"{manager.FirstName} {manager.LastName}",
                ManagerEmail = manager.EMail,
                TeamSize = teamMembers.Count,
                TotalAssignments = teamAssignments.Count,
                CompletedAssignments = teamCompleted,
                OverdueAssignments = teamOverdue,
                CompletionRate = Math.Round(teamCompletionRate, 1)
            });
        }

        // Recent activity (last 7 days)
        var sevenDaysAgo = now.AddDays(-7);
        var assignmentsCreatedLast7Days = allAssignments.Count(a => a.AssignedDate >= sevenDaysAgo);
        var assignmentsCompletedLast7Days = allAssignments.Count(a =>
            a.CompletedDate.HasValue && a.CompletedDate.Value >= sevenDaysAgo);

        // Urgent assignments (due within 3 days or overdue)
        var urgentThreshold = now.AddDays(3);
        var urgentAssignments = allAssignments
            .Where(a => a.DueDate.HasValue && a.DueDate.Value <= urgentThreshold && a.WorkflowState != WorkflowState.Finalized)
            .OrderBy(a => a.DueDate)
            .Take(20)
            .Select(a =>
            {
                var employee = allEmployees.FirstOrDefault(e => e.Id == a.EmployeeId);
                var manager = employee != null
                    ? allEmployees.FirstOrDefault(m => m.EmployeeId == employee.ManagerId)
                    : null;
                var organization = employee != null
                    ? allOrganizations.FirstOrDefault(o => int.Parse(o.Number) == employee.OrganizationNumber)
                    : null;

                var daysUntilDue = (int)(a.DueDate!.Value - now).TotalDays;
                var isOverdue = a.DueDate!.Value < now;

                return new UrgentAssignment
                {
                    AssignmentId = a.Id,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.EmployeeName,
                    ManagerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : "Unknown",
                    QuestionnaireTemplateName = templateLookup.TryGetValue(a.TemplateId, out var name) ? name : "Unknown Template",
                    DueDate = a.DueDate.Value,
                    WorkflowState = a.WorkflowState.ToString(),
                    IsOverdue = isOverdue,
                    DaysUntilDue = daysUntilDue,
                    OrganizationName = organization?.Name ?? "Unknown"
                };
            })
            .ToList();

        return new HRDashboardReadModel
        {
            Id = Guid.NewGuid(), // System-level dashboard
            TotalEmployees = totalEmployees,
            TotalManagers = totalManagers,
            TotalAssignments = totalAssignments,
            TotalPendingAssignments = pendingAssignments,
            TotalInProgressAssignments = inProgressAssignments,
            TotalCompletedAssignments = completedAssignments,
            TotalOverdueAssignments = overdueAssignments,
            OverallCompletionRate = Math.Round(completionRate, 1),
            AverageCompletionTimeInDays = Math.Round(averageCompletionTime, 1),
            Organizations = organizationMetrics,
            Managers = managerOverviews.OrderByDescending(m => m.TeamSize).ToList(),
            AssignmentsCreatedLast7Days = assignmentsCreatedLast7Days,
            AssignmentsCompletedLast7Days = assignmentsCompletedLast7Days,
            UrgentAssignments = urgentAssignments,
            LastUpdated = DateTime.UtcNow
        };
    }
}
