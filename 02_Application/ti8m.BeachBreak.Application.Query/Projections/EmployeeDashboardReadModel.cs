using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Application.Query.Projections;

/// <summary>
/// Read model for Employee Dashboard view.
/// Aggregates metrics and urgent items per employee from QuestionnaireAssignmentReadModel documents.
/// This is populated on-demand by querying assignment data rather than via event sourcing projections.
/// </summary>
public class EmployeeDashboardReadModel
{
    public Guid Id { get; set; } // EmployeeId
    public string EmployeeFullName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;

    // Aggregate Metrics
    public int PendingCount { get; set; } = 0;
    public int InProgressCount { get; set; } = 0;
    public int CompletedCount { get; set; } = 0;

    // Urgent Items (assignments with DueDate within 3 days or overdue)
    public List<UrgentAssignmentItem> UrgentAssignments { get; set; } = new();

    // Last Updated
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public class UrgentAssignmentItem
    {
        public Guid AssignmentId { get; set; }
        public string QuestionnaireTemplateName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public WorkflowState WorkflowState { get; set; }
        public bool IsOverdue { get; set; }
    }
}
