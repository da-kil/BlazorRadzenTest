namespace ti8m.BeachBreak.Client.Models;

public class AssignmentFilter
{
    public AssignmentStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Department { get; set; }
    public string? EmployeeId { get; set; }
    public bool IncludeTeamOnly { get; set; } = false;
    public bool IncludeAccessibleOnly { get; set; } = true;
}

public class AnalyticsQuery
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Department { get; set; }
    public AnalyticsScope Scope { get; set; } = AnalyticsScope.Accessible;
    public int TrendDays { get; set; } = 30;
}

public enum AnalyticsScope
{
    Own,        // Employee's own data
    Team,       // Manager's team
    Department, // Department-wide (HR)
    Organization, // Organization-wide (HR/Admin)
    Accessible  // Based on user role and permissions
}

public class NotificationRequest
{
    public List<Guid> AssignmentIds { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Reminder;
    public DateTime? ScheduledFor { get; set; }
}

public enum NotificationType
{
    Reminder,
    Deadline,
    Completion,
    Custom
}

// Analytics Response Models
public interface IAnalytics
{
    DateTime GeneratedAt { get; set; }
    string Scope { get; set; }
}

public class PersonalAnalytics : IAnalytics
{
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public string Scope { get; set; } = "Personal";
    public int CompletedQuestionnaires { get; set; }
    public int PendingQuestionnaires { get; set; }
    public int OverdueQuestionnaires { get; set; }
    public double AverageCompletionTime { get; set; }
    public List<CompetencyScore> CompetencyScores { get; set; } = new();
}

public class TeamAnalytics : IAnalytics
{
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public string Scope { get; set; } = "Team";
    public int TeamSize { get; set; }
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int PendingAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public double CompletionRate { get; set; }
    public List<TeamMemberProgress> TeamProgress { get; set; } = new();
    public List<CompetencyTrend> CompetencyTrends { get; set; } = new();
}

public class OrganizationAnalytics : IAnalytics
{
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public string Scope { get; set; } = "Organization";
    public int TotalEmployees { get; set; }
    public int TotalQuestionnaires { get; set; }
    public int CompletedQuestionnaires { get; set; }
    public double OverallCompletionRate { get; set; }
    public List<DepartmentAnalytics> DepartmentBreakdown { get; set; } = new();
    public List<TrendData> CompletionTrends { get; set; } = new();
    public ComplianceMetrics Compliance { get; set; } = new();
}

public class DepartmentAnalytics : IAnalytics
{
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public string Scope { get; set; } = "Department";
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public int CompletedAssignments { get; set; }
    public int PendingAssignments { get; set; }
    public double CompletionRate { get; set; }
    public List<CompetencyScore> AverageCompetencyScores { get; set; } = new();
}

// Supporting Models
public class CompetencyScore
{
    public string CompetencyKey { get; set; } = string.Empty;
    public string CompetencyName { get; set; } = string.Empty;
    public double AverageScore { get; set; }
    public int ResponseCount { get; set; }
}

public class TeamMemberProgress
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int CompletedAssignments { get; set; }
    public int PendingAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public double ProgressPercentage { get; set; }
}

public class CompetencyTrend
{
    public string CompetencyKey { get; set; } = string.Empty;
    public string CompetencyName { get; set; } = string.Empty;
    public List<TrendData> TrendData { get; set; } = new();
}

public class TrendData
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class ComplianceMetrics
{
    public int TotalDueAssignments { get; set; }
    public int CompletedOnTime { get; set; }
    public int OverdueAssignments { get; set; }
    public double ComplianceRate { get; set; }
    public List<ComplianceByDepartment> DepartmentCompliance { get; set; } = new();
}

public class ComplianceByDepartment
{
    public string Department { get; set; } = string.Empty;
    public int TotalAssignments { get; set; }
    public int CompletedOnTime { get; set; }
    public double ComplianceRate { get; set; }
}

// Report Models
public abstract class BaseReport
{
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public string GeneratedBy { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class TeamPerformanceReport : BaseReport
{
    public string TeamName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public List<TeamMemberProgress> TeamMembers { get; set; } = new();
    public List<CompetencyScore> TeamCompetencyAverages { get; set; } = new();
}

public class OrganizationReport : BaseReport
{
    public OrganizationAnalytics Analytics { get; set; } = new();
    public List<DepartmentAnalytics> DepartmentReports { get; set; } = new();
    public ComplianceReport Compliance { get; set; } = new();
}

public class ComplianceReport : BaseReport
{
    public ComplianceMetrics Metrics { get; set; } = new();
    public List<AssignmentComplianceDetail> Details { get; set; } = new();
}

public class AssignmentComplianceDetail
{
    public Guid AssignmentId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string QuestionnaireName { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public AssignmentStatus Status { get; set; }
    public bool IsCompliant { get; set; }
    public int DaysOverdue { get; set; }
}

// Assignment Progress Model (moved from IEmployeeQuestionnaireService)
public class AssignmentProgress
{
    public Guid AssignmentId { get; set; }
    public int ProgressPercentage { get; set; }
    public int TotalQuestions { get; set; }
    public int AnsweredQuestions { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsCompleted { get; set; }
    public TimeSpan? TimeSpent { get; set; }
}