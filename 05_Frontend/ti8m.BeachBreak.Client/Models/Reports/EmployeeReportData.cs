namespace ti8m.BeachBreak.Client.Models.Reports;

/// <summary>
/// Report data structure for individual employee questionnaire metrics.
/// Contains assignment statistics and completion rates for a single employee.
/// Includes department information for organization-wide reporting.
/// </summary>
public class EmployeeReportData
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Department { get; set; } = "";
    public string Role { get; set; } = "";
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int PendingAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public double CompletionPercentage { get; set; }
}