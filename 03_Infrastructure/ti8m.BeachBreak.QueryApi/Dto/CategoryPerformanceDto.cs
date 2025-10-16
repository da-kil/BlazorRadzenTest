namespace ti8m.BeachBreak.QueryApi.Dto;

public class CategoryPerformanceDto
{
    public string Category { get; set; } = string.Empty;
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionTime { get; set; }
}
