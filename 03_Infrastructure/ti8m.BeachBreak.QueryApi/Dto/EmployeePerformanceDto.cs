namespace ti8m.BeachBreak.QueryApi.Dto;

public class EmployeePerformanceDto
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionTime { get; set; }
}
