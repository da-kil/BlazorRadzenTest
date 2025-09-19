namespace ti8m.BeachBreak.Application.Query.Queries.AnalyticsQueries;

public class OrganizationAnalytics
{
    public int TotalEmployees { get; set; }
    public int TotalActiveAssignments { get; set; }
    public int TotalCompletedAssignments { get; set; }
    public int TotalOverdueAssignments { get; set; }
    public decimal OverallCompletionRate { get; set; }
    public TimeSpan? AverageCompletionTime { get; set; }
    public Dictionary<string, DepartmentMetrics> DepartmentBreakdown { get; set; } = new();
    public List<MonthlyTrend> MonthlyTrends { get; set; } = new();
}

public class DepartmentMetrics
{
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public int AssignmentCount { get; set; }
    public decimal CompletionRate { get; set; }
}

public class MonthlyTrend
{
    public DateTime Month { get; set; }
    public int AssignmentsCreated { get; set; }
    public int AssignmentsCompleted { get; set; }
    public decimal CompletionRate { get; set; }
}