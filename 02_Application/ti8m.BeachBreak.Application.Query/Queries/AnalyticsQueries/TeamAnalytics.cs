namespace ti8m.BeachBreak.Application.Query.Queries.AnalyticsQueries;

public class TeamAnalytics
{
    public int TotalTeamMembers { get; set; }
    public int ActiveAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public TimeSpan? AverageCompletionTime { get; set; }
    public decimal CompletionRate { get; set; }
    public Dictionary<string, object> TeamPerformanceMetrics { get; set; } = new();
}