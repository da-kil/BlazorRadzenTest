namespace ti8m.BeachBreak.Application.Query.Queries.AnalyticsQueries;

public class DepartmentAnalytics
{
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int ActiveAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public decimal CompletionRate { get; set; }
    public TimeSpan? AverageCompletionTime { get; set; }
    public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
}