namespace ti8m.BeachBreak.QueryApi.Dto;

public class EmployeeDashboardDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeFullName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;

    // Aggregate Metrics
    public int PendingCount { get; set; } = 0;
    public int InProgressCount { get; set; } = 0;
    public int CompletedCount { get; set; } = 0;

    // Urgent Assignments (due within 3 days or overdue)
    public List<UrgentAssignmentDto> UrgentAssignments { get; set; } = new();

    public DateTime LastUpdated { get; set; }
}

public class UrgentAssignmentDto
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
