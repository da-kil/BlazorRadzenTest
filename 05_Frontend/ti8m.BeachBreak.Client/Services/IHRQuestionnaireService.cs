using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface IHRQuestionnaireService
{
    Task<List<EmployeeDto>> GetAllEmployeesAsync();
    Task<List<QuestionnaireAssignment>> GetAllAssignmentsAsync();
    Task<List<QuestionnaireAssignment>> GetAssignmentsByDepartmentAsync(string department);
    Task<List<QuestionnaireAssignment>> GetAssignmentsByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<OrganizationAnalytics> GetOrganizationAnalyticsAsync();
    Task<List<DepartmentAnalytics>> GetDepartmentAnalyticsAsync();
    Task<ComplianceReport> GetComplianceReportAsync();
    Task<OrganizationReport> GenerateOrganizationReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> SendBulkReminderAsync(List<Guid> assignmentIds, string message);
    Task<List<QuestionnaireTemplate>> GetQuestionnaireUsageStatsAsync();
    Task<List<TrendData>> GetCompletionTrendsAsync(int days = 30);
}

public class OrganizationAnalytics
{
    public int TotalEmployees { get; set; }
    public int TotalQuestionnaires { get; set; }
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public double OrganizationCompletionRate { get; set; }
    public double AverageCompletionTime { get; set; }
    public double OnTimeCompletionRate { get; set; }
    public List<DepartmentPerformance> DepartmentPerformance { get; set; } = new();
    public List<QuestionnairePopularity> QuestionnairePopularity { get; set; } = new();
    public List<TrendData> CompletionTrends { get; set; } = new();
}

public class DepartmentAnalytics
{
    public string Department { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionTime { get; set; }
    public List<EmployeePerformance> TopPerformers { get; set; } = new();
    public List<EmployeePerformance> NeedsAttention { get; set; } = new();
}

public class DepartmentPerformance
{
    public string Department { get; set; } = string.Empty;
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionTime { get; set; }
    public string PerformanceLevel { get; set; } = string.Empty; // Excellent, Good, Fair, Needs Attention
}

public class QuestionnairePopularity
{
    public Guid QuestionnaireId { get; set; }
    public string QuestionnaireName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionTime { get; set; }
    public double AverageRating { get; set; }
}

public class TrendData
{
    public DateTime Date { get; set; }
    public int CompletedCount { get; set; }
    public int AssignedCount { get; set; }
    public double CompletionRate { get; set; }
}

public class ComplianceReport
{
    public DateTime GeneratedDate { get; set; } = DateTime.Now;
    public int TotalEmployees { get; set; }
    public int EmployeesWithAssignments { get; set; }
    public int EmployeesCompliant { get; set; }
    public double ComplianceRate { get; set; }
    public List<ComplianceItem> RequiredQuestionnaires { get; set; } = new();
    public List<EmployeeCompliance> NonCompliantEmployees { get; set; } = new();
    public List<DepartmentCompliance> DepartmentCompliance { get; set; } = new();
}

public class ComplianceItem
{
    public Guid QuestionnaireId { get; set; }
    public string QuestionnaireName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public DateTime? ComplianceDeadline { get; set; }
    public int TotalRequired { get; set; }
    public int Completed { get; set; }
    public double ComplianceRate { get; set; }
}

public class EmployeeCompliance
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public List<Guid> MissingQuestionnaires { get; set; } = new();
    public List<Guid> OverdueQuestionnaires { get; set; } = new();
    public double ComplianceScore { get; set; }
}

public class DepartmentCompliance
{
    public string Department { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int CompliantEmployees { get; set; }
    public double ComplianceRate { get; set; }
    public List<ComplianceItem> DepartmentRequirements { get; set; } = new();
}

public class OrganizationReport
{
    public DateTime GeneratedDate { get; set; } = DateTime.Now;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public OrganizationAnalytics Analytics { get; set; } = new();
    public ComplianceReport Compliance { get; set; } = new();
    public List<DepartmentAnalytics> DepartmentAnalytics { get; set; } = new();
    public List<QuestionnaireAssignment> Assignments { get; set; } = new();
}