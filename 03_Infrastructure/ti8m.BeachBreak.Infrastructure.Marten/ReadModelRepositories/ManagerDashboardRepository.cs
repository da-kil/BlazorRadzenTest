using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

internal class ManagerDashboardRepository(IDocumentStore store) : IManagerDashboardRepository
{
    public async Task<ManagerDashboardReadModel?> GetDashboardByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();

        // Get the manager's employee record
        var manager = await session.Query<EmployeeReadModel>()
            .FirstOrDefaultAsync(e => e.Id == managerId, cancellationToken);

        if (manager == null)
        {
            return null; // Manager not found
        }

        // Get all direct reports (team members)
        var teamMembers = await session.Query<EmployeeReadModel>()
            .Where(e => e.ManagerId == managerId.ToString() && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!teamMembers.Any())
        {
            // Manager has no team members, return empty dashboard
            return new ManagerDashboardReadModel
            {
                ManagerId = managerId,
                ManagerFullName = $"{manager.FirstName} {manager.LastName}",
                ManagerEmail = manager.EMail,
                TeamMemberCount = 0,
                LastUpdated = DateTime.UtcNow
            };
        }

        var teamMemberIds = teamMembers.Select(e => e.Id).ToList();

        // Query all assignments for all team members
        var allAssignments = await session.Query<QuestionnaireAssignmentReadModel>()
            .Where(a => teamMemberIds.Contains(a.EmployeeId))
            .ToListAsync(cancellationToken);

        if (!allAssignments.Any())
        {
            // No assignments for team yet
            return new ManagerDashboardReadModel
            {
                ManagerId = managerId,
                ManagerFullName = $"{manager.FirstName} {manager.LastName}",
                ManagerEmail = manager.EMail,
                TeamMemberCount = teamMembers.Count,
                TeamMembers = teamMembers.Select(tm => new ManagerDashboardReadModel.TeamMemberMetrics
                {
                    EmployeeId = tm.Id,
                    EmployeeName = $"{tm.FirstName} {tm.LastName}",
                    EmployeeEmail = tm.EMail
                }).ToList(),
                LastUpdated = DateTime.UtcNow
            };
        }

        // Get unique template IDs for name lookup
        var templateIds = allAssignments.Select(a => a.TemplateId).Distinct().ToList();
        var templates = await session.Query<QuestionnaireTemplateReadModel>()
            .Where(t => templateIds.Contains(t.Id))
            .ToListAsync(cancellationToken);
        var templateDict = templates.ToDictionary(t => t.Id, t => t.Name);

        // Calculate team-wide metrics
        var teamPendingCount = allAssignments.Count(a => a.WorkflowState == WorkflowState.Assigned);
        var teamInProgressCount = allAssignments.Count(a =>
            a.WorkflowState == WorkflowState.EmployeeInProgress ||
            a.WorkflowState == WorkflowState.ManagerInProgress ||
            a.WorkflowState == WorkflowState.BothInProgress);
        var teamCompletedCount = allAssignments.Count(a => a.WorkflowState == WorkflowState.Finalized);

        // Calculate per-team-member metrics
        var now = DateTime.UtcNow;
        var teamMemberMetrics = new List<ManagerDashboardReadModel.TeamMemberMetrics>();

        foreach (var teamMember in teamMembers)
        {
            var memberAssignments = allAssignments.Where(a => a.EmployeeId == teamMember.Id).ToList();

            var pendingCount = memberAssignments.Count(a => a.WorkflowState == WorkflowState.Assigned);
            var inProgressCount = memberAssignments.Count(a =>
                a.WorkflowState == WorkflowState.EmployeeInProgress ||
                a.WorkflowState == WorkflowState.ManagerInProgress ||
                a.WorkflowState == WorkflowState.BothInProgress);
            var completedCount = memberAssignments.Count(a => a.WorkflowState == WorkflowState.Finalized);

            var urgentAssignments = memberAssignments.Where(a =>
                a.DueDate.HasValue &&
                (a.DueDate.Value - now).TotalDays <= 3 &&
                a.WorkflowState != WorkflowState.Finalized).ToList();

            var hasOverdue = urgentAssignments.Any(a => a.DueDate!.Value < now);

            teamMemberMetrics.Add(new ManagerDashboardReadModel.TeamMemberMetrics
            {
                EmployeeId = teamMember.Id,
                EmployeeName = $"{teamMember.FirstName} {teamMember.LastName}",
                EmployeeEmail = teamMember.EMail,
                PendingCount = pendingCount,
                InProgressCount = inProgressCount,
                CompletedCount = completedCount,
                UrgentCount = urgentAssignments.Count,
                HasOverdueItems = hasOverdue
            });
        }

        // Find urgent assignments across the entire team
        var teamUrgentAssignments = allAssignments
            .Where(a => a.DueDate.HasValue &&
                        (a.DueDate.Value - now).TotalDays <= 3 &&
                        a.WorkflowState != WorkflowState.Finalized)
            .Select(a =>
            {
                var employee = teamMembers.First(e => e.Id == a.EmployeeId);
                var daysUntilDue = (int)Math.Ceiling((a.DueDate!.Value - now).TotalDays);

                return new ManagerDashboardReadModel.TeamUrgentAssignment
                {
                    AssignmentId = a.Id,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.EmployeeName,
                    QuestionnaireTemplateName = templateDict.GetValueOrDefault(a.TemplateId, "Unknown"),
                    DueDate = a.DueDate!.Value,
                    WorkflowState = a.WorkflowState,
                    IsOverdue = a.DueDate!.Value < now,
                    DaysUntilDue = daysUntilDue
                };
            })
            .OrderBy(a => a.DueDate)
            .ToList();

        return new ManagerDashboardReadModel
        {
            ManagerId = managerId,
            ManagerFullName = $"{manager.FirstName} {manager.LastName}",
            ManagerEmail = manager.EMail,
            TeamPendingCount = teamPendingCount,
            TeamInProgressCount = teamInProgressCount,
            TeamCompletedCount = teamCompletedCount,
            TeamMemberCount = teamMembers.Count,
            TeamMembers = teamMemberMetrics,
            UrgentAssignments = teamUrgentAssignments,
            LastUpdated = DateTime.UtcNow
        };
    }
}
