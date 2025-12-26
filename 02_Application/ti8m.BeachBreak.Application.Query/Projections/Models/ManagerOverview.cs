namespace ti8m.BeachBreak.Application.Query.Projections.Models;

/// <summary>
/// Overview data for manager statistics in HR dashboard.
/// Extracted from HRDashboardReadModel for proper file organization.
/// </summary>
public class ManagerOverview
{
    public Guid ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public string ManagerEmail { get; set; } = string.Empty;
    public int TeamSize { get; set; } = 0;
    public int TotalAssignments { get; set; } = 0;
    public int CompletedAssignments { get; set; } = 0;
    public int OverdueAssignments { get; set; } = 0;
    public double CompletionRate { get; set; } = 0.0;
}