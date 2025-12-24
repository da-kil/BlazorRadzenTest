namespace ti8m.BeachBreak.Application.Query.Projections.Models;

/// <summary>
/// Metrics data for individual team members in manager dashboard.
/// Extracted from ManagerDashboardReadModel for proper file organization.
/// </summary>
public class TeamMemberMetrics
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public int PendingCount { get; set; } = 0;
    public int InProgressCount { get; set; } = 0;
    public int CompletedCount { get; set; } = 0;
    public int UrgentCount { get; set; } = 0;
    public bool HasOverdueItems { get; set; } = false;
}