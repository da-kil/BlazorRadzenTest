namespace ti8m.BeachBreak.Application.Query.Queries.AnalyticsQueries;

public class TrendData
{
    public DateOnly Date { get; set; }
    public decimal CompletionRate { get; set; }
    public int NewAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public TimeSpan? AverageResponseTime { get; set; }
}