namespace ti8m.BeachBreak.Client.Models.Reports;

/// <summary>
/// Report data structure for individual team member questionnaire metrics.
/// Contains assignment statistics and completion rates for a single team member.
/// </summary>
public class TeamMemberReportData
{
    public Guid MemberId { get; set; }
    public string MemberName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int PendingAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public double CompletionPercentage { get; set; }
}