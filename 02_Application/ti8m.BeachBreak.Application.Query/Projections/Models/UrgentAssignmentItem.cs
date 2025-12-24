using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Application.Query.Projections.Models;

/// <summary>
/// Data for urgent assignment items in employee dashboard.
/// Extracted from EmployeeDashboardReadModel for proper file organization.
/// </summary>
public class UrgentAssignmentItem
{
    public Guid AssignmentId { get; set; }
    public string QuestionnaireTemplateName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public WorkflowState WorkflowState { get; set; }
    public bool IsOverdue { get; set; }
}