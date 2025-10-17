using ti8m.BeachBreak.Core.Domain.SharedKernel;

namespace ti8m.BeachBreak.Application.Query.Projections;

/// <summary>
/// Read model for Manager Dashboard view.
/// Aggregates metrics and urgent items for all team members (direct reports) from QuestionnaireAssignmentReadModel documents.
/// This is populated on-demand by querying assignment data for all team members.
/// </summary>
public class ManagerDashboardReadModel
{
    public Guid ManagerId { get; set; }
    public string ManagerFullName { get; set; } = string.Empty;
    public string ManagerEmail { get; set; } = string.Empty;

    // Team-wide Aggregate Metrics
    public int TeamPendingCount { get; set; } = 0;
    public int TeamInProgressCount { get; set; } = 0;
    public int TeamCompletedCount { get; set; } = 0;
    public int TeamMemberCount { get; set; } = 0;

    // Team Members with their individual metrics
    public List<TeamMemberMetrics> TeamMembers { get; set; } = new();

    // Urgent assignments across the team
    public List<TeamUrgentAssignment> UrgentAssignments { get; set; } = new();

    // Last Updated
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public class TeamMemberMetrics
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public int PendingCount { get; set; } = 0;
        public int InProgressCount { get; set; } = 0;
        public int CompletedCount { get; set; } = 0;
        public int UrgentCount { get; set; } = 0;
        public bool HasOverdueItems { get; set; } = false;
    }

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
}
