using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface IManagerQuestionnaireService
{
    Task<List<EmployeeDto>> GetTeamMembersAsync();
    Task<List<QuestionnaireAssignment>> GetTeamAssignmentsAsync();
    Task<List<QuestionnaireAssignment>> GetTeamAssignmentsByStatusAsync(AssignmentStatus status);
    Task<List<AssignmentProgress>> GetTeamProgressAsync();
    Task<TeamAnalytics> GetTeamAnalyticsAsync();
    Task<List<QuestionnaireAssignment>> GetAssignmentsByEmployeeAsync(string employeeId);
    Task<bool> SendReminderAsync(Guid assignmentId, string message);
    Task<TeamPerformanceReport> GenerateTeamReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
}

public class TeamAnalytics
{
    public int TotalTeamMembers { get; set; }
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public double AverageCompletionTime { get; set; }
    public double OnTimeCompletionRate { get; set; }
    public List<CategoryPerformance> CategoryPerformance { get; set; } = new();
    public List<EmployeePerformance> EmployeePerformance { get; set; } = new();
}

public class CategoryPerformance
{
    public string Category { get; set; } = string.Empty;
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionTime { get; set; }
}

public class EmployeePerformance
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionTime { get; set; }
}

public class TeamPerformanceReport
{
    public DateTime GeneratedDate { get; set; } = DateTime.Now;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public TeamAnalytics Analytics { get; set; } = new();
    public List<QuestionnaireAssignment> Assignments { get; set; } = new();
    public List<EmployeeDto> TeamMembers { get; set; } = new();
}