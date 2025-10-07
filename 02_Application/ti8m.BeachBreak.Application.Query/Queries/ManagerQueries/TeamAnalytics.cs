namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

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
