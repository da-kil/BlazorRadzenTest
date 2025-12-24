namespace ti8m.BeachBreak.Application.Query.Projections.Models;

/// <summary>
/// Metrics data for organization-level dashboard statistics.
/// Extracted from HRDashboardReadModel for proper file organization.
/// </summary>
public class OrganizationMetrics
{
    public int OrganizationNumber { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; } = 0;
    public int TotalAssignments { get; set; } = 0;
    public int PendingCount { get; set; } = 0;
    public int InProgressCount { get; set; } = 0;
    public int CompletedCount { get; set; } = 0;
    public int OverdueCount { get; set; } = 0;
    public double CompletionRate { get; set; } = 0.0;
}