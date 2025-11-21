using System.Text;

namespace ti8m.BeachBreak.Client.Models.Reports;

/// <summary>
/// Report data structure for team questionnaire analytics.
/// Contains overall team metrics and individual team member details.
/// </summary>
public class TeamReportData : BaseQuestionnaireReportData
{
    public int TotalTeamMembers { get; set; }
    public List<TeamMemberReportData> TeamMemberDetails { get; set; } = new();

    /// <summary>
    /// Generates the CSV header section for team reports.
    /// </summary>
    /// <returns>CSV header content including team-specific metadata</returns>
    public override string GetCsvHeader()
    {
        var commonHeader = GetCommonHeaderLines("Team Questionnaires Report");
        return $@"{commonHeader}
Total Team Members: {TotalTeamMembers}";
    }

    /// <summary>
    /// Generates the CSV data rows for team member details.
    /// </summary>
    /// <returns>Enumerable of CSV data rows for team members</returns>
    public override IEnumerable<string> GetCsvDetailRows()
    {
        yield return "Team Member,Email,Role,Total Assignments,Completed,Pending,Overdue,Completion %";

        foreach (var member in TeamMemberDetails)
        {
            yield return $"\"{member.MemberName}\",\"{member.Email}\",\"{member.Role}\",{member.TotalAssignments},{member.CompletedAssignments},{member.PendingAssignments},{member.OverdueAssignments},{member.CompletionPercentage:F1}%";
        }
    }
}