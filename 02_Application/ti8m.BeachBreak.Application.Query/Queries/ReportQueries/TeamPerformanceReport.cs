namespace ti8m.BeachBreak.Application.Query.Queries.ReportQueries;

public class TeamPerformanceReport
{
    public string ReportPeriod { get; set; } = string.Empty;
    public Dictionary<string, object> TeamMetrics { get; set; } = new();
    public List<IndividualPerformance> IndividualPerformances { get; set; } = new();
    public Dictionary<string, object> TrendAnalysis { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class IndividualPerformance
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int CompletedAssignments { get; set; }
    public decimal CompletionRate { get; set; }
    public TimeSpan? AverageCompletionTime { get; set; }
    public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
}