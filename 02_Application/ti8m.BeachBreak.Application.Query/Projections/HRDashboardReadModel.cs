using ti8m.BeachBreak.Application.Query.Projections.Models;

namespace ti8m.BeachBreak.Application.Query.Projections;

public class HRDashboardReadModel
{
    public Guid Id { get; set; } // HR User ID or system ID

    // Organization-wide Metrics
    public int TotalEmployees { get; set; } = 0;
    public int TotalManagers { get; set; } = 0;
    public int TotalAssignments { get; set; } = 0;
    public int TotalPendingAssignments { get; set; } = 0;
    public int TotalInProgressAssignments { get; set; } = 0;
    public int TotalCompletedAssignments { get; set; } = 0;
    public int TotalOverdueAssignments { get; set; } = 0;

    // Completion Metrics
    public double OverallCompletionRate { get; set; } = 0.0;
    public double AverageCompletionTimeInDays { get; set; } = 0.0;

    // Department/Organization Breakdown
    public List<OrganizationMetrics> Organizations { get; set; } = new();

    // Manager Overview
    public List<ManagerOverview> Managers { get; set; } = new();

    // Recent Activity
    public int AssignmentsCreatedLast7Days { get; set; } = 0;
    public int AssignmentsCompletedLast7Days { get; set; } = 0;

    // Urgent Items
    public List<UrgentAssignment> UrgentAssignments { get; set; } = new();

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
