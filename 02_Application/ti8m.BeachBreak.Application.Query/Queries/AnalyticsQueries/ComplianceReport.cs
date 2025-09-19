namespace ti8m.BeachBreak.Application.Query.Queries.AnalyticsQueries;

public class ComplianceReport
{
    public decimal ComplianceScore { get; set; }
    public int TotalRequiredAssignments { get; set; }
    public int CompletedRequiredAssignments { get; set; }
    public int OverdueRequiredAssignments { get; set; }
    public List<NonCompliantEmployee> NonCompliantEmployees { get; set; } = new();
    public Dictionary<string, decimal> DepartmentCompliance { get; set; } = new();
    public DateTime ReportGeneratedDate { get; set; }
}

public class NonCompliantEmployee
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int OverdueCount { get; set; }
}