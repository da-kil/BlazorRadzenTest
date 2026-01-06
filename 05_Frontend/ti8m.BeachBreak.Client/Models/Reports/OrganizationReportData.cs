using System.Text;

namespace ti8m.BeachBreak.Client.Models.Reports;

/// <summary>
/// Report data structure for organization-wide questionnaire analytics.
/// Contains overall organization metrics and individual employee details.
///
/// NOTE: If section-level details are added to this report in the future,
/// ensure custom sections (IsInstanceSpecific = true) are filtered out to
/// maintain consistency across questionnaire instances.
/// </summary>
public class OrganizationReportData : BaseQuestionnaireReportData
{
    public int TotalEmployees { get; set; }
    public List<EmployeeReportData> EmployeeDetails { get; set; } = new();

    /// <summary>
    /// Generates the CSV header section for organization reports.
    /// </summary>
    /// <returns>CSV header content including organization-specific metadata</returns>
    public override string GetCsvHeader()
    {
        var commonHeader = GetCommonHeaderLines("Organization Questionnaires Report");
        return $@"{commonHeader}
Total Employees: {TotalEmployees}";
    }

    /// <summary>
    /// Generates the CSV data rows for employee details.
    /// </summary>
    /// <returns>Enumerable of CSV data rows for employees</returns>
    public override IEnumerable<string> GetCsvDetailRows()
    {
        yield return "Employee,Email,Department,Role,Total Assignments,Completed,Pending,Overdue,Completion %";

        foreach (var employee in EmployeeDetails)
        {
            yield return $"\"{employee.EmployeeName}\",\"{employee.Email}\",\"{employee.Department}\",\"{employee.Role}\",{employee.TotalAssignments},{employee.CompletedAssignments},{employee.PendingAssignments},{employee.OverdueAssignments},{employee.CompletionPercentage:F1}%";
        }
    }
}