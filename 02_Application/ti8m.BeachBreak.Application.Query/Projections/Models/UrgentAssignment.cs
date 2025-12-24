namespace ti8m.BeachBreak.Application.Query.Projections.Models;

/// <summary>
/// Data for urgent assignment items in HR dashboard.
/// Extracted from HRDashboardReadModel for proper file organization.
/// </summary>
public class UrgentAssignment
{
    public Guid AssignmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public string QuestionnaireTemplateName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string WorkflowState { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
    public int DaysUntilDue { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
}