using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Application.Query.Projections.Models;

/// <summary>
/// Data for urgent assignment items in manager dashboard.
/// Extracted from ManagerDashboardReadModel for proper file organization.
/// </summary>
public class TeamUrgentAssignment
{
    public Guid AssignmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string QuestionnaireTemplateName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public WorkflowState WorkflowState { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysUntilDue { get; set; }
}