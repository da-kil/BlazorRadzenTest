namespace ti8m.BeachBreak.QueryApi.Dto;

public class OrganizationAnalyticsDto
{
    public int TotalEmployees { get; set; }
    public int TotalActiveAssignments { get; set; }
    public int TotalCompletedAssignments { get; set; }
    public int TotalOverdueAssignments { get; set; }
    public decimal OverallCompletionRate { get; set; }
    public TimeSpan? AverageCompletionTime { get; set; }
    public Dictionary<string, DepartmentMetricsDto> DepartmentBreakdown { get; set; } = new();
    public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
}

public class DepartmentMetricsDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public int AssignmentCount { get; set; }
    public decimal CompletionRate { get; set; }
}

public class MonthlyTrendDto
{
    public DateTime Month { get; set; }
    public int AssignmentsCreated { get; set; }
    public int AssignmentsCompleted { get; set; }
    public decimal CompletionRate { get; set; }
}

public class DepartmentAnalyticsDto
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

public class ComplianceReportDto
{
    public decimal ComplianceScore { get; set; }
    public int TotalRequiredAssignments { get; set; }
    public int CompletedRequiredAssignments { get; set; }
    public int OverdueRequiredAssignments { get; set; }
    public List<NonCompliantEmployeeDto> NonCompliantEmployees { get; set; } = new();
    public Dictionary<string, decimal> DepartmentCompliance { get; set; } = new();
    public DateTime ReportGeneratedDate { get; set; }
}

public class NonCompliantEmployeeDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int OverdueCount { get; set; }
}

public class OrganizationReportDto
{
    public string ReportPeriod { get; set; } = string.Empty;
    public string ExecutiveSummary { get; set; } = string.Empty;
    public Dictionary<string, object> DepartmentPerformance { get; set; } = new();
    public Dictionary<string, object> OverallMetrics { get; set; } = new();
    public Dictionary<string, object> TrendAnalysis { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public Dictionary<string, object> DetailedBreakdown { get; set; } = new();
}

public class QuestionnaireUsageStatsDto
{
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public TimeSpan? AverageCompletionTime { get; set; }
    public decimal CompletionRate { get; set; }
    public DateTime? LastUsedDate { get; set; }
    public decimal PopularityScore { get; set; }
}

public class TrendDataDto
{
    public DateTime Date { get; set; }
    public decimal CompletionRate { get; set; }
    public int NewAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public TimeSpan? AverageResponseTime { get; set; }
}