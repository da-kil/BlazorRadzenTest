namespace ti8m.BeachBreak.Application.Query.Queries.ReportQueries;

public class OrganizationReport
{
    public string ReportPeriod { get; set; } = string.Empty;
    public string ExecutiveSummary { get; set; } = string.Empty;
    public Dictionary<string, object> DepartmentPerformance { get; set; } = new();
    public Dictionary<string, object> OverallMetrics { get; set; } = new();
    public Dictionary<string, object> TrendAnalysis { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public Dictionary<string, object> DetailedBreakdown { get; set; } = new();
}