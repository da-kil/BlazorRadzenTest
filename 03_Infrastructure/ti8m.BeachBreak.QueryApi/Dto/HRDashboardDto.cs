namespace ti8m.BeachBreak.QueryApi.Dto;

public class HRDashboardDto
{
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
    public List<OrganizationMetricsDto> Organizations { get; set; } = new();

    // Manager Overview
    public List<ManagerOverviewDto> Managers { get; set; } = new();

    // Recent Activity
    public int AssignmentsCreatedLast7Days { get; set; } = 0;
    public int AssignmentsCompletedLast7Days { get; set; } = 0;

    // Urgent Items
    public List<UrgentAssignmentDto> UrgentAssignments { get; set; } = new();

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class OrganizationMetricsDto
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

public class ManagerOverviewDto
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
